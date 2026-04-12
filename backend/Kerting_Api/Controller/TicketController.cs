using Kerting_Api.DTO;
using Libary.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kerting_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketController : ControllerBase
    {
        private readonly Libary.KertingDbContext _context;

        public TicketController(Libary.KertingDbContext context)
        {
            _context = context;
        }

        // 1. ÚJ HIBAJEGY BEKÜLDÉSE
        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] TicketCreateDto dto)
        {
            var userIdString = User.FindFirst("Id")?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Hibás token: Nem található az Id!");
            }

            var ticket = new Ticket
            {
                Title = dto.Title,
                Description = dto.Description,
                UserId = userId,
                CreatedAt = DateTime.Now,
                IsResolved = false
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Hibajegy sikeresen elküldve!" });
        }

        // 2. ÖSSZES HIBAJEGY LEKÉRÉSE (Javítva: 500-as hiba kiküszöbölve!)
        [HttpGet]
        public async Task<IActionResult> GetAllTickets()
        {
            // PONTOSAN AZ A JAVÍTÁS, MINT A LÉTREHOZÁSNÁL
            var userIdString = User.FindFirst("Id")?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Hibás token: Nem található az Id!");
            }

            // Ellenőrizzük, hogy Admin-e (roleId == 1)
            var currentUser = await _context.User.FindAsync(userId);
            if (currentUser == null || currentUser.RoleId != 1)
            {
                return Forbid("Nincs jogosultságod ehhez a funkcióhoz!");
            }

            // Lekérjük a ticketeket (Közben a mezőneveket kisbetűvel adjuk át a Vue-nak, hogy biztosan passzoljon)
            var tickets = await (from t in _context.Tickets
                                 join u in _context.User on t.UserId equals u.Id
                                 orderby t.CreatedAt descending
                                 select new
                                 {
                                     id = t.Id,
                                     title = t.Title,
                                     description = t.Description,
                                     createdAt = t.CreatedAt,
                                     isResolved = t.IsResolved,
                                     bekuldoNeve = u.VezetekNev + " " + u.KeresztNev,
                                     bekuldoAvatar = u.IMGString,
                                     userId = u.Id
                                 }).ToListAsync();

            return Ok(tickets);
        }

        // 3. HIBAJEGY LEZÁRÁSA
        [HttpPut("{id}/resolve")]
        public async Task<IActionResult> ResolveTicket(int id)
        {
            // ITT IS JAVÍTVA AZ ID OLVASÁS
            var userIdString = User.FindFirst("Id")?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Hibás token!");
            }

            var currentUser = await _context.User.FindAsync(userId);
            if (currentUser == null || currentUser.RoleId != 1) return Forbid();

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            ticket.IsResolved = true; // Átállítjuk lezártra
            await _context.SaveChangesAsync();

            return Ok(new { message = "Hibajegy lezárva." });
        }
    }
}