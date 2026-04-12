using Kerting_Api.Interface;
using Kerting_Api.Model.Gallery;
using Libary.Model.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Kerting_Api.Controller
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Galéria végpontok: feedek, profilkép kezelés, item CRUD, publikáció, reakciók és kommentek.
    /// </summary>
    public class GalleryController : ControllerBase
    {
        private readonly IGalleryService _galleryService;
        private readonly IWebHostEnvironment _env;

        public GalleryController(IGalleryService galleryService, IWebHostEnvironment env)
        {
            _galleryService = galleryService;
            _env = env;
        }

        // User ID kiolvasása a token claim-ből.
        private int GetCurrentUserId() => int.Parse(User.FindFirst("Id")?.Value ?? "0");

        private int? TryGetCurrentUserId()
        {
            var rawId = User.FindFirst("Id")?.Value;
            return int.TryParse(rawId, out var userId) ? userId : null;
        }

        /// <summary>
        /// Publikus galéria feed lapozott lekérése.
        /// </summary>
        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool includeDeleted = false)
        {
            return Ok(await _galleryService.GetGalleryFeedAsync(page, pageSize, TryGetCurrentUserId(), includeDeleted));
        }

        /// <summary>
        /// Egy konkrét user publikus galéria feedje.
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserFeed(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool includeDeleted = false)
        {
            return Ok(await _galleryService.GetUserGalleryFeedAsync(userId, page, pageSize, TryGetCurrentUserId(), includeDeleted));
        }

        /// <summary>
        /// Bejelentkezett user saját galéria feedje.
        /// </summary>
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool includeDeleted = false)
        {
            var userId = GetCurrentUserId();
            return Ok(await _galleryService.GetOwnGalleryFeedAsync(userId, page, pageSize, userId, includeDeleted));
        }

        /// <summary>
        /// Galéria item lekérése ID alapján.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, [FromQuery] bool includeDeleted = false)
        {
            var item = await _galleryService.GetGalleryItemByIdAsync(id, TryGetCurrentUserId(), includeDeleted);
            return item is null ? NotFound() : Ok(item);
        }

        /// <summary>
        /// Profilkép feltöltése a bejelentkezett userhez.
        /// </summary>
        [HttpPost("profile-image")]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            try
            {
                var url = await _galleryService.UploadProfileImageAsync(GetCurrentUserId(), file, _env.ContentRootPath ?? _env.WebRootPath ?? ".");
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Profilkép törlése.
        /// </summary>
        [HttpDelete("profile-image")]
        public async Task<IActionResult> DeleteProfileImage()
        {
            try
            {
                var success = await _galleryService.DeleteProfileImageAsync(GetCurrentUserId(), _env.ContentRootPath ?? _env.WebRootPath ?? ".");
                if (!success) return NotFound("Nem található törölhető profilkép.");

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Új galéria item feltöltése (kép + metadata).
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] string title, [FromForm] string description, IFormFile file)
        {
            try
            {
                var result = await _galleryService.UploadItemAsync(GetCurrentUserId(), title, description, file, _env.ContentRootPath);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        /// <summary>
        /// Galéria item törlése.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _galleryService.DeleteItemAsync(id, GetCurrentUserId(), _env.ContentRootPath);
            return success ? Ok() : NotFound();
        }

        /// <summary>
        /// Törölt galéria item visszaállítása.
        /// </summary>
        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            var success = await _galleryService.RestoreItemAsync(id, GetCurrentUserId());
            return success ? Ok() : NotFound();
        }

        /// <summary>
        /// Galéria item metadata frissítési kérésadat.
        /// </summary>
        public sealed class UpdateGalleryItemRequest
        {
            [MaxLength(150)]
            public string Title { get; set; } = string.Empty;

            [MaxLength(2000)]
            public string? Description { get; set; }
        }

        /// <summary>
        /// Galéria item title/leírás frissítése.
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGalleryItemRequest request)
        {
            var success = await _galleryService.UpdateItemAsync(id, GetCurrentUserId(), request.Title, request.Description ?? string.Empty);
            return success ? Ok() : NotFound();
        }

        /// <summary>
        /// Publikációs állapot váltás.
        /// </summary>
        [HttpPatch("{id}/publish")]
        public async Task<IActionResult> SetPublishState(int id, [FromQuery] bool isPublished)
        {
            var success = await _galleryService.SetPublishStateAsync(id, GetCurrentUserId(), isPublished);
            return success ? Ok() : NotFound();
        }

        /// <summary>
        /// Like/dislike reakció váltás item szinten.
        /// </summary>
        [HttpPost("{id}/react")]
        public async Task<IActionResult> ToggleReaction(int id, [FromQuery] bool isLike)
        {
            var success = await _galleryService.ToggleReactionAsync(id, GetCurrentUserId(), isLike);
            return success ? Ok() : NotFound();
        }

        /// <summary>
        /// Új komment felvétele egy itemhez.
        /// </summary>
        [HttpPost("{id}/comment")]
        public async Task<IActionResult> AddComment(int id, [FromBody] CommentRequest request)
        {
            try
            {
                var comment = await _galleryService.AddCommentAsync(id, GetCurrentUserId(), request.Message);
                var response = new CommentResponse
                {
                    Id = comment.Id,
                    GalleryItemId = comment.GalleryItemId,
                    UserId = comment.UserId,
                    Message = comment.Message,
                    CreatedAtUtc = comment.CreatedAtUtc,
                    IsDeleted = comment.IsDeleted
                };
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Komment törlése.
        /// </summary>
        [HttpDelete("comment/{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var success = await _galleryService.DeleteCommentAsync(commentId, GetCurrentUserId());
            return success ? Ok() : NotFound();
        }

        /// <summary>
        /// Törölt komment visszaállítása.
        /// </summary>
        [HttpPatch("comment/{commentId}/restore")]
        public async Task<IActionResult> RestoreComment(int commentId)
        {
            var success = await _galleryService.RestoreCommentAsync(commentId, GetCurrentUserId());
            return success ? Ok() : NotFound();
        }
    }
}