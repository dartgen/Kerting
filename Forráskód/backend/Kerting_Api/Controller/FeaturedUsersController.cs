using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kerting_Api.DTO;
using Kerting_Api.Interface;

namespace Kerting_Api.Controller
{
    [Route("api/featured-users")]
    [ApiController]
    /// <summary>
    /// Kiemelt felhasználó végpontok (publikus carousel + admin slotkezelés).
    /// </summary>
    public class FeaturedUsersController : ControllerBase
    {
        private readonly IFeaturedUsersService _featuredUsersService;

        public FeaturedUsersController(IFeaturedUsersService featuredUsersService)
        {
            _featuredUsersService = featuredUsersService;
        }

        /// <summary>
        /// Főoldali carouselhez szükséges kiemelt felhasználók listája.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetFeaturedUsersForCarousel()
        {
            var result = await _featuredUsersService.GetFeaturedUsersForCarouselAsync();
            return Ok(result);
        }

        /// <summary>
        /// Admin szerkesztő felület adatforrása (slotok + választható user lista).
        /// </summary>
        [Authorize]
        [HttpGet("admin/data")]
        public async Task<IActionResult> GetAdminFeaturedData()
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Forbid();
            }

            try
            {
                var result = await _featuredUsersService.GetAdminFeaturedDataAsync(currentUserId.Value);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Kiemelt slotok mentése admin jogosultsággal.
        /// </summary>
        [Authorize]
        [HttpPut("admin/slots")]
        public async Task<IActionResult> UpsertFeaturedSlots([FromBody] List<FeaturedSlotUpsertDto>? request)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Forbid();
            }

            try
            {
                await _featuredUsersService.UpsertFeaturedSlotsAsync(currentUserId.Value, request ?? new List<FeaturedSlotUpsertDto>());
                return Ok(new { Message = "Kiemelt felhasználók sikeresen frissítve." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Segédfüggvény a token alapján olvasott user ID visszaadására.
        private int? GetCurrentUserId()
        {
            var userIdString = User.FindFirst("Id")?.Value;
            if (!int.TryParse(userIdString, out var userId))
            {
                return null;
            }
            return userId;
        }
    }
}
