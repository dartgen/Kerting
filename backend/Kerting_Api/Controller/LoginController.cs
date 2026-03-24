using Libary; // Hivatkozás a másik projektre, ahol a DbContext van.
using Libary.Model.Auth; // Hivatkozás a Login adatmodellre.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; // Ez kell ahhoz, hogy ez egy API Controller lehessen (kezeli a HTTP kéréseket).
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // Ez tartalmazza a kriptográfiai kulcsokat és algoritmusokat (a titkosításhoz).
using System.IdentityModel.Tokens.Jwt; // Ez a konkrét eszköz a JWT tokenek gyártásához és olvasásához.
using System.Security.Claims; // A "Claim" egy adatmorzsa a felhasználóról (pl. neve, ID-ja), amit a tokenbe írunk.
using System.Text; // Ez kell a szöveg átalakításához bájtokká (Encoding.UTF8).

namespace Kerting_Api.Controller
{   
    //TODO
    //Megcsinálni tökenek rendes használatát pl ha interactol valamit akkor frisül mondjuk a token, ne lehesen egy falhasználónak több tokenje!

    [Route("api")] // Ez határozza meg az URL-t. Mivel az osztály neve LoginController, a cím: .../api/Login
    [ApiController] // Ez jelzi a .NET-nek, hogy ez nem weboldal, hanem API (automatikusan ellenőrzi a bejövő adatokat).
    public class LoginController : ControllerBase
    {
        // Két privát változó, amiket később használunk:
        private readonly KertingDbContext _context; // Ez a kapcsolat az adatbázissal.
        private readonly IConfiguration _configuration; // Ez a kapcsolat az appsettings.json fájllal.

        // KONSTRUKTOR (Dependency Injection)
        // Amikor a szerver elindítja ezt a Controllert, automatikusan beadja neki (injektálja)
        // az adatbázist és a konfigurációt. Nem nekünk kell példányosítani őket a 'new' kulcsszóval.
        public LoginController(KertingDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("Login")] // Ez mondja meg, hogy ez egy POST kérés lesz, a címe: api/Login/Authenticate
        // A "Task<IActionResult>" azt jelenti, hogy aszinkron (nem fagyasztja le a szervert várakozás közben).
        // A "[FromBody] Login loginAdatok" azt jelenti: vedd a JSON-t a kérés testéből, és alakítsd át Login objektummá.
        public async Task<IActionResult> Login([FromBody] DummyLogin loginAdatok)
        {
            // 1. ADATBÁZIS LEKÉRDEZÉS
            // A "_context.Logins" a Logins táblát jelenti.
            // A "FirstOrDefaultAsync" egy SQL lekérdezést futtat le:
            // "SELECT * FROM Logins WHERE Username = '...' AND Password = '...' LIMIT 1"
            var user = await _context.Login
                .FirstOrDefaultAsync(u => u.Username == loginAdatok.Username);

            // 2. ELLENŐRZÉS
            // Ha a "user" változó null (üres), az azt jelenti, hogy nincs ilyen felhasználó vagy rossz a jelszó.
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginAdatok.Password, user.Password))
            {
                // A Verify metódus:
                // - Első paraméter: amit most beírt a user (sima szöveg)
                // - Második paraméter: ami az adatbázisban van (hash)
                return Unauthorized("Hibás felhasználónév vagy jelszó.");
            }

            // 3. HA SIKERES: Token igénylés
            // Meghívjuk a saját metódusunkat (lentebb), ami legyártja a hosszú stringet (a tokent).
            var token = GenerateJwtToken(user);

            // 4. VÁLASZ
            // A 200 OK kódot küldjük vissza, benne egy JSON objektummal: { "token": "eyBgf..." }
            return Ok(new
            {
                token = token,
                message = "Sikeres bejelentkezés"
            });
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] DummyLogin loginAdatok)
        {
            // 1. JELSZÓ HASH-ELÉSE
            // Ez a sor csinálja a varázslatot. A sima szövegből (loginAdatok.Password)
            // egy biztonságos hash-t készít.
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(loginAdatok.Password);

            // 2. Új user létrehozása a hash-elt jelszóval
            var newUser = new Login
            {
                Username = loginAdatok.Username,
                Password = passwordHash, // FONTOS: A hash-t mentjük, nem az eredetit!
                                         // Egyéb mezők...
                RoleId = 2
            };

            _context.Login.Add(newUser);
            await _context.SaveChangesAsync();
            return Ok("Sikeres regisztráció!" + $"\n{newUser.Username}\n{newUser.Password}");
        }

        [Authorize]
        [HttpGet("{id}/first-login")]
        public async Task<IActionResult> CheckFirstLogin(int id)
        {
            // 1. Kikeresjük a felhasználót az adatbázisból az ID alapján
            var user = await _context.Login.FindAsync(id);
            // 2. Visszaadjuk a FirstLogin értékét egy JSON objektumban
            return Ok(new { isFirstLogin = user.FirstLogin });
        }

        [HttpGet("CheckUsername")]
        public async Task<IActionResult> CheckUsername([FromQuery] string username)
        {
            // Megnézzük az adatbázisban, hogy van-e már ilyen Username
            bool foglalt = await _context.Login.AnyAsync(u => u.Username.ToLower() == username.ToLower());

            // Visszaküldünk egy JSON objektumot, amit a Vue vár (isTaken: true/false)
            return Ok(new { isTaken = foglalt });
        }
        private string GenerateJwtToken(Login user)
        {
            // Beolvassuk a beállításokat a "JwtSettings" szekcióból az appsettings.json-ből.
            var jwtSettings = _configuration.GetSection("JwtSettings");

            // A titkos szöveges kulcsot (amit az előbb javítottál ki hosszabbra) átalakítjuk bájtokká.
            // A számítógép kriptográfiai műveletekhez csak bájtokat ért, stringet nem.
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            // CLAIMS (Állítások) LISTA:
            // Ezek az adatok bele lesznek "égetve" a tokenbe. Bárki elolvashatja őket, de nem tudja módosítani.
            var claims = new List<Claim>
            {
                // "Sub" (Subject): A token tulajdonosa (a felhasználónév).
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                
                // "Jti": A token egyedi azonosítója (egy véletlen GUID). Biztonsági okokból jó.
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                
                // Egyedi adat: Az adatbázisbeli ID-t is eltároljuk, hátha később kell a frontendnek.
                new Claim("Id", user.Id.ToString()),
            };

            // TOKEN LEÍRÓ (A tervrajz):
            // Itt állítjuk össze a token minden paraméterét.
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // A payload (a fenti adatok listája)
                Subject = new ClaimsIdentity(claims),

                // Lejárat: Mostantól számítva 2 óra múlva érvénytelen lesz.
                Expires = DateTime.UtcNow.AddHours(2),

                // Kibocsátó: "KertingApi" (az appsettings-ből olvassa)
                Issuer = jwtSettings["Issuer"],

                // Közönség: "KertingClient" (kinek szól a token)
                Audience = jwtSettings["Audience"],

                // ALÁÍRÁS (A legfontosabb rész):
                // Itt használjuk a HMACSHA256 algoritmust és a titkos kulcsunkat ("key").
                // Ez garantálja, hogy a tokent mi állítottuk ki, és senki nem piszkált bele.
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            // Végül a "TokenHandler" legyártja a tényleges tokent a tervrajz alapján.
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // A kész objektumot átalakítjuk egy hosszú stringgé (pl. "eyJhbGc...") és visszaadjuk.
            return tokenHandler.WriteToken(token);
        }
    }
}