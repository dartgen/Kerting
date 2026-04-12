using Kerting_Api.Interface;
using Libary;
using Libary.Model.Auth;
using Libary.Model.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Kerting_Api.Service
{
    /// <summary>
    /// A bejelentkezéshez és regisztrációhoz tartozó üzleti szabályok implementációja.
    /// Feladata a hitelesítési adatok ellenőrzése, jelszó-hashelés és JWT token kiadása.
    /// </summary>
    public sealed class AuthService : IAuthService
    {
        private readonly KertingDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(KertingDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Felhasználónév alapján kikeresi a fiókot, ellenőrzi a BCrypt hash-t,
        /// majd sikeres azonosításkor visszaad egy rövid lejáratú JWT tokent.
        /// </summary>
        public async Task<string> LoginAsync(DummyLogin loginAdatok)
        {
            var user = await _context.Login.FirstOrDefaultAsync(u => u.Username == loginAdatok.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginAdatok.Password, user.Password))
            {
                throw new UnauthorizedAccessException("Hibás felhasználónév vagy jelszó.");
            }

            return GenerateJwtToken(user);
        }

        /// <summary>
        /// Új login rekordot hoz létre hash-elt jelszóval, majd automatikusan
        /// létrehozza a kapcsolódó User profilt alap szerepkörrel (RoleId = 2).
        /// </summary>
        public async Task RegisterAsync(DummyLogin loginAdatok)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(loginAdatok.Password);

            var newUser = new Login
            {
                Username = loginAdatok.Username,
                Password = passwordHash,
            };

            _context.Login.Add(newUser);
            await _context.SaveChangesAsync();
            _context.User.Add(new User
            {
                Id = newUser.Id,
                RoleId = 2
            });
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Kezeli az "első belépés" jelzőt.
        /// Ha az érték igaz, a hívás hamisra állítja és visszaadja, hogy ez tényleg első alkalom volt-e.
        /// </summary>
        public async Task<bool> HandleFirstLoginAsync(int id)
        {
            var user = await _context.Login.FindAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException();
            }

            var wasFirstLogin = user.FirstLogin;
            if (wasFirstLogin)
            {
                user.FirstLogin = false;
                await _context.SaveChangesAsync();
            }

            return wasFirstLogin;
        }

        /// <summary>
        /// Felhasználónév foglaltság ellenőrzése kis/nagybetű-független módon.
        /// </summary>
        public async Task<bool> CheckUsernameAsync(string username)
        {
            return await _context.Login.AnyAsync(u => u.Username.ToLower() == username.ToLower());
        }

        /// <summary>
        /// JWT token generálás a bejelentkezett user alapadataiból.
        /// A token claim-jei:
        /// - sub: felhasználónév
        /// - jti: egyedi token azonosító
        /// - Id: rendszerbeli user azonosító
        /// </summary>
        private string GenerateJwtToken(Login user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            // A claim-eket a frontend és backend végpontok is felhasználják
            // userazonosításra és alap audit jellegű követésre.
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("Id", user.Id.ToString()),
            };

            // A SecurityTokenDescriptor adja meg a token érvényességi és aláírási paramétereit.
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            // A token végső string formájában kerül vissza a kliensnek.
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}