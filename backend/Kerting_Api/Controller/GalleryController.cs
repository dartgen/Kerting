using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Kerting_Api.Interface;

namespace Kerting_Api.Controller
{
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

        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var feed = await _galleryService.GetGalleryFeedAsync(page, pageSize);
            return Ok(feed);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItem(int id)
        {
            var item = await _galleryService.GetGalleryItemByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] int userId, [FromForm] string title, [FromForm] string description, IFormFile file)
        {
            try
            {
                var result = await _galleryService.UploadItemAsync(userId, title, description, file, _env.ContentRootPath ?? _env.WebRootPath ?? ".");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] int userId)
        {
            var success = await _galleryService.DeleteItemAsync(id, userId, _env.ContentRootPath ?? _env.WebRootPath ?? ".");
            if (!success) return NotFound("Item not found or unauthorised.");
            return Ok();
        }

        [HttpPost("{id}/react")]
        public async Task<IActionResult> ToggleReaction(int id, [FromQuery] int userId, [FromQuery] bool isLike)
        {
            await _galleryService.ToggleReactionAsync(id, userId, isLike);
            return Ok();
        }

        [HttpPost("{id}/comment")]
        public async Task<IActionResult> AddComment(int id, [FromQuery] int userId, [FromBody] string message)
        {
            try
            {
                var comment = await _galleryService.AddCommentAsync(id, userId, message);
                return Ok(comment);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("comment/{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId, [FromQuery] int userId)
        {
            var success = await _galleryService.DeleteCommentAsync(commentId, userId);
            if (!success) return NotFound();
            return Ok();
        }
    }
}