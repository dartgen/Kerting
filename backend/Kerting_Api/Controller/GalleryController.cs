using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Kerting_Api.Interface;

namespace Kerting_Api.Controller
{
    [Authorize] // Minden végponthoz kell Token
    [Route("api/[controller]")]
    [ApiController]
    public class GalleryController : ControllerBase
    {
        private readonly IGalleryService _galleryService;
        private readonly IWebHostEnvironment _env;

        public GalleryController(IGalleryService galleryService, IWebHostEnvironment env)
        {
            _galleryService = galleryService;
            _env = env;
        }

        // Segédmetódus a UserId kinyeréséhez a Tokenből
        private int GetCurrentUserId() => int.Parse(User.FindFirst("Id")?.Value ?? "0");

        [HttpGet("feed")]
        [AllowAnonymous] // A feedet bárki láthatja
        public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            return Ok(await _galleryService.GetGalleryFeedAsync(page, pageSize));
        }

        // --- PROFILKÉP KEZELÉS ---

        [HttpPost("profile-image")]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            try
            {
                var url = await _galleryService.UploadProfileImageAsync(GetCurrentUserId(), file, _env.ContentRootPath);
                return Ok(new { url });
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpDelete("profile-image")]
        public async Task<IActionResult> DeleteProfileImage()
        {
            await _galleryService.DeleteProfileImageAsync(GetCurrentUserId(), _env.ContentRootPath);
            return Ok();
        }

        // --- GALÉRIA KEZELÉS ---

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _galleryService.DeleteItemAsync(id, GetCurrentUserId(), _env.ContentRootPath);
            return success ? Ok() : NotFound();
        }

        [HttpPost("{id}/react")]
        public async Task<IActionResult> ToggleReaction(int id, [FromQuery] bool isLike)
        {
            await _galleryService.ToggleReactionAsync(id, GetCurrentUserId(), isLike);
            return Ok();
        }

        [HttpPost("{id}/comment")]
        public async Task<IActionResult> AddComment(int id, [FromBody] string message)
        {
            var comment = await _galleryService.AddCommentAsync(id, GetCurrentUserId(), message);
            return Ok(comment);
        }
    }
}