using Libary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kerting_Api.Controller
{
    [Route("api/[controller]")]
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
            existingUser.Email = updatedUser.Email;
            existingUser.Telepules = updatedUser.Telepules;
            existingUser.ProfileIMGId = updatedUser.ProfileIMGId;
            existingUser.Rolam = updatedUser.Rolam;

            await _context.SaveChangesAsync();

            return Ok("A profil adatai sikeresen frissítve lettek!");
        }
    }
}
