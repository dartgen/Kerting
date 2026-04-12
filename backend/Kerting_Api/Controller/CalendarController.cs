using Microsoft.AspNetCore.Mvc;
using Kerting_Api.Interface;
using Libary.Model.Project;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Kerting_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    /// <summary>
    /// Személyes naptárbejegyzés végpontok.
    /// </summary>
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarService _calendarService;

        public CalendarController(ICalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        /// <summary>
        /// Bejelentkezett user saját naptárbejegyzéseinek listázása.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyEntries()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var entries = await _calendarService.GetEntriesByUserIdAsync(userId);
            return Ok(entries);
        }

        /// <summary>
        /// Új naptárbejegyzés mentése a bejelentkezett userhez rendelve.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveEntry([FromBody] CalendarEntry entry)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            entry.UserId = userId;
            var savedEntry = await _calendarService.CreateEntryAsync(entry);
            return Ok(savedEntry);
        }

        /// <summary>
        /// Saját naptárbejegyzés törlése ID alapján.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntry(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _calendarService.DeleteEntryAsync(id, userId);
            return Ok();
        }
    }
}