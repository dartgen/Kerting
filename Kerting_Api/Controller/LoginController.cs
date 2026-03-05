using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Libary; // A DbContext miatt
using Libary.Model.Auth; // A Login modell miatt
using Microsoft.EntityFrameworkCore;

namespace Kerting_Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly KertingDbContext _context;
        private readonly IConfiguration _configuration;

        // Dependency Injection: Itt kapjuk meg az adatbázist és a beállításokat
        public LoginController(KertingDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("Authenticate")]
        public async Task<IActionResult> Login([FromBody] Login loginAdatok)
        {
            // 1. Megkeressük a felhasználót az adatbázisban
            // Feltételezem, a Login osztályban van 'Username' és 'Password' mező
            var user = await _context.Login
                .FirstOrDefaultAsync(u => u.Username == loginAdatok.Username && u.Password == loginAdatok.Password);

            // MEGJEGYZÉS: Élesben a jelszót sose hasonlítsd össze simán (==), 
            // hanem hash-eld! (pl. BCrypt), de első körben a működéshez ez is jó.

            if (user == null)
            {
                return Unauthorized("Hibás felhasználónév vagy jelszó.");
            }

            // 2. Token generálás
            var token = GenerateJwtToken(user);

            return Ok(new { token = token });
        }

        private string GenerateJwtToken(Login user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("Id", user.Id.ToString()) // Ha van ID-ja
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2), // 2 óráig érvényes
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}