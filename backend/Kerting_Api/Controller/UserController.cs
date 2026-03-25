using Libary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kerting_Api.Controller
{
    [Route("api")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly KertingDbContext _context;

        public UserController(KertingDbContext context)
        {
            _context = context;
        }

        [Authorize] // Kötelező a bejelentkezés
        [HttpPut("UpdateMyProfile")] // Figyeld meg: kivettem az {id}-t az URL-ből!
        public async Task<IActionResult> UpdateMyProfile([FromBody] Libary.Model.User.User updatedUser)
        {
            // 1. Kiszedjük a bejelentkezett felhasználó ID-ját a Tokenből (Claims)
            // A ClaimTypes.NameIdentifier az a szabványos hely, ahova az ID-t rakni szoktuk a token generálásakor
            var userIdString = User.FindFirst("Id")?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int loggedInUserId))
            {
                return Unauthorized("Érvénytelen token vagy hiányzó azonosító.");
            }

            // 2. KIKERESÉS A TOKENBŐL KAPOTT ID ALAPJÁN (Ezt már nem tudja meghamisítani)
            var existingUser = await _context.User.FindAsync(loggedInUserId);

            if (existingUser == null)
            {
                return NotFound("Felhasználó nem található.");
            }

            // 3. Adatok felülírása (Ugyanaz, mint eddig)
            existingUser.VezetekNev = updatedUser.VezetekNev;
            existingUser.KeresztNev = updatedUser.KeresztNev;
            existingUser.Telefon = updatedUser.Telefon;
            existingUser.Email = updatedUser.Email;
            existingUser.Telepules = updatedUser.Telepules;
            existingUser.Rolam = updatedUser.Rolam;

            await _context.SaveChangesAsync();

            return Ok("A profil adatai sikeresen frissítve lettek!");
        }

        [Authorize] // Ide is kötelező a bejelentkezés
        [HttpGet("GetMyProfile")] // Ez egy GET kérés lesz
        public async Task<IActionResult> GetMyProfile()
        {
            // 1. Kiszedjük a bejelentkezett felhasználó ID-ját a Tokenből
            // (Ha korábban a ClaimTypes.NameIdentifier-t választottad, azt írd ide!)
            var userIdString = User.FindFirst("Id")?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int loggedInUserId))
            {
                return Unauthorized("Érvénytelen token vagy hiányzó azonosító.");
            }

            // 2. Kikeresjük a felhasználót az adatbázisból
            var userProfile = await _context.User.FindAsync(loggedInUserId);

            // 3. Visszaadjuk a teljes User objektumot a frontendnek (JSON formátumban)
            return Ok(userProfile);
        }

        [Authorize]
        [HttpGet("GetRoles")]
        public async Task<IActionResult> GetRoles()
        {
            // Lekérdezzük az összes szerepkört az adatbázisból
            var roles = await _context.Role.ToListAsync();
            return Ok(roles);
        }
    }
}
