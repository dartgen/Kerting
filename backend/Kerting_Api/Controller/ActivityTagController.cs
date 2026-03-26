using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Libary;

namespace Kerting_Api.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityTagController : ControllerBase
    {
        private readonly KertingDbContext _context;

        public ActivityTagController(KertingDbContext context)
        {
            _context = context;
        }

        // 1. GET ALL végpont (Bárki elérheti, aki be van jelentkezve)
        // GET: api/ActivityTag
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Lekérjük az összes elemet az adatbázisból
            var tags = await _context.ActivityTag.Select(x => x.Activity).ToListAsync();
            return Ok(tags);
        }

        [Authorize]
        [HttpDelete("{name}")]
        public async Task<IActionResult> DeleteByName(string name)
        {
            // 1. Kiszedjük a bejelentkezett felhasználó ID-ját a Tokenből
            var userIdString = User.FindFirst("Id")?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int loggedInUserId))
            {
                return Unauthorized("Érvénytelen token vagy hiányzó azonosító.");
            }

            // 2. Kikeresjük a felhasználót az adatbázisból
            var userProfile = await _context.User.FindAsync(loggedInUserId);

            // Biztonsági ellenőrzés: Mi van, ha a token jó, de a usert közben törölték az adatbázisból?
            if (userProfile == null)
            {
                return Unauthorized("A felhasználó nem található az adatbázisban.");
            }

            // 3. Valós idejű jogosultság ellenőrzése (RoleId == 1 az Admin)
            if (userProfile.RoleId != 1)
            {
                // 403 Forbidden: Be van jelentkezve, de ehhez a konkrét művelethez nincs joga.
                return Forbid();
            }

            // 5. Megkeressük az adatbázisban a taget a neve alapján
            var tagToDelete = await _context.ActivityTag
                                            .FirstOrDefaultAsync(t => t.Activity == name);

            // 6. Ha nincs ilyen, 404 Not Found hibát dobunk
            if (tagToDelete == null)
            {
                return NotFound($"Nem található ilyen nevű tevékenység: {name}");
            }

            // 7. Törlés és mentés
            _context.ActivityTag.Remove(tagToDelete);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"A '{name}' nevű tevékenység sikeresen törölve lett." });
        }
    }
}
