using Kerting_Api.DTO;
using Kerting_Api.Interface;
using Libary.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kerting_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    /// <summary>
    /// Hibajegy végpontok: létrehozás, admin listázás, lezárás.
    /// </summary>
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        /// <summary>
        /// Új hibajegy beküldése a bejelentkezett usertől.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] TicketCreateDto dto)
        {
            var userIdString = User.FindFirst("Id")?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Hibás token: Nem található az Id!");
            }

            await _ticketService.CreateTicketAsync(userId, dto);

            return Ok(new { message = "Hibajegy sikeresen elküldve!" });
        }

        /// <summary>
        /// Összes hibajegy listázása admin joggal.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllTickets()
        {
            // User ID tokenből, ugyanazzal a mintával mint a create végpontnál.
            var userIdString = User.FindFirst("Id")?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Hibás token: Nem található az Id!");
            }

            try
            {
                var tickets = await _ticketService.GetAllTicketsAsync(userId);
                return Ok(tickets);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// Hibajegy lezárás admin jogosultsággal.
        /// </summary>
        [HttpPut("{id}/resolve")]
        public async Task<IActionResult> ResolveTicket(int id)
        {
            // User ID ellenőrzés a token claim-ből.
            var userIdString = User.FindFirst("Id")?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Hibás token!");
            }

            try
            {
                await _ticketService.ResolveTicketAsync(id, userId);
                return Ok(new { message = "Hibajegy lezárva." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}