using Libary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Kerting_Api.DTO;
using Kerting_Api.Interface;

namespace Kerting_Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Felhasználói értékelés végpontok: listázás, beküldés, reakció, törlés és visszaállítás.
    /// </summary>
    public class UserReviewController : ControllerBase
    {
        private readonly IUserReviewService _userReviewService;

        public UserReviewController(IUserReviewService userReviewService)
        {
            _userReviewService = userReviewService;
        }

        // A claimből biztonságosan kiolvasott aktuális user azonosító.
        private int? TryGetCurrentUserId()
        {
            var rawId = User.FindFirst("Id")?.Value;
            return int.TryParse(rawId, out var userId) ? userId : null;
        }

        /// <summary>
        /// Egy userhez tartozó értékelésfolyam lekérése.
        /// Publikus, de bejelentkezve a saját reakció is visszaadható.
        /// </summary>
        [HttpGet("{targetUserId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviews(int targetUserId)
        {
            var currentUserId = TryGetCurrentUserId();
            var result = await _userReviewService.GetReviewsAsync(targetUserId, currentUserId);
            return Ok(result);
        }

        /// <summary>
        /// Új értékelés vagy válasz beküldése.
        /// </summary>
        [Authorize]
        [HttpPost("{targetUserId}")]
        public async Task<IActionResult> AddReview(int targetUserId, [FromBody] AddReviewRequest request)
        {
            try
            {
                var userId = TryGetCurrentUserId()!.Value;
                await _userReviewService.AddReviewAsync(targetUserId, userId, request);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Like/dislike reakció váltás egy adott értékelésen.
        /// </summary>
        [Authorize]
        [HttpPost("{reviewId}/react")]
        public async Task<IActionResult> ToggleReaction(int reviewId, [FromQuery] bool isLike)
        {
            try
            {
                var userId = TryGetCurrentUserId()!.Value;
                await _userReviewService.ToggleReactionAsync(reviewId, userId, isLike);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Értékelés törlése jogosultság-ellenőrzéssel.
        /// A service kezeli a soft/hard delete szabalyokat.
        /// </summary>
        [Authorize]
        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            try
            {
                var userId = TryGetCurrentUserId()!.Value;
                await _userReviewService.DeleteReviewAsync(reviewId, userId);
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Korábban törölt értékelés visszaállítása.
        /// </summary>
        [Authorize]
        [HttpPatch("{reviewId}/restore")]
        public async Task<IActionResult> RestoreReview(int reviewId)
        {
            try
            {
                var userId = TryGetCurrentUserId()!.Value;
                await _userReviewService.RestoreReviewAsync(reviewId, userId);
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
