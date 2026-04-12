using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Kerting_Api.Interface;

namespace Kerting_Api.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    /// <summary>
    /// Activity tag végpontok.
    /// </summary>
    public class ActivityTagController : ControllerBase
    {
        private readonly IActivityTagService _activityTagService;

        public ActivityTagController(IActivityTagService activityTagService)
        {
            _activityTagService = activityTagService;
        }

        /// <summary>
        /// Összes elérhető activity címke listázása.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tags = await _activityTagService.GetAllAsync();
            return Ok(tags);
        }

        /// <summary>
        /// Címke törlése név alapján.
        /// Csak admin felhasználó törölhet.
        /// </summary>
        [Authorize]
        [HttpDelete("{name}")]
        public async Task<IActionResult> DeleteByName(string name)
        {
            // User ID kiolvasása a token claim-ből.
            var userIdString = User.FindFirst("Id")?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int loggedInUserId))
            {
                return Unauthorized("Érvénytelen token vagy hiányzó azonosító.");
            }

            try
            {
                await _activityTagService.DeleteByNameAsync(name, loggedInUserId);
                return Ok(new { Message = $"A '{name}' nevű tevékenység sikeresen törölve lett." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
