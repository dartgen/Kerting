using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Kerting_Api.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kerting_Api.Controller
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ForumController : ControllerBase
    {
        private readonly IForumService _forumService;

        public ForumController(IForumService forumService)
        {
            _forumService = forumService;
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirst("Id")?.Value ?? "0");

        private int? TryGetCurrentUserId()
        {
            var rawId = User.FindFirst("Id")?.Value;
            return int.TryParse(rawId, out var userId) ? userId : null;
        }

        public sealed class UpsertForumPostRequest
        {
            [Required]
            [MaxLength(150)]
            public string Title { get; set; } = string.Empty;

            [MaxLength(2000)]
            public string Description { get; set; } = string.Empty;

            public int? AttachedGalleryItemId { get; set; }
            public List<string>? Tags { get; set; }
        }

        public sealed class AddCommentRequest
        {
            [Required]
            [MaxLength(1000)]
            public string Message { get; set; } = string.Empty;

            public int? ParentCommentId { get; set; }
        }

        public sealed class LockStateRequest
        {
            public bool IsLocked { get; set; }
            [MaxLength(300)]
            public string? Reason { get; set; }
        }

        [HttpGet("feed")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFeed(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sort = "latest",
            [FromQuery] string? search = null,
            [FromQuery] int? maxAgeDays = 30,
            [FromQuery] List<int>? roleIds = null,
            [FromQuery] List<string>? tagNames = null,
            [FromQuery] bool includeDeleted = false)
        {
            var result = await _forumService.GetFeedAsync(
                page,
                pageSize,
                TryGetCurrentUserId(),
                sort,
                search,
                maxAgeDays,
                roleIds,
                tagNames,
                includeDeleted);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(
            int id,
            [FromQuery] bool includeDeleted = false,
            [FromQuery] int commentCursor = 0,
            [FromQuery] int commentPageSize = 20,
            [FromQuery] int replyPageSize = 5)
        {
            var result = await _forumService.GetPostByIdAsync(
                id,
                TryGetCurrentUserId(),
                includeDeleted,
                commentCursor,
                commentPageSize,
                replyPageSize);

            return result is null ? NotFound() : Ok(result);
        }

        [HttpGet("comment/{commentId}/replies")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReplies(
            int commentId,
            [FromQuery] int cursor = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeDeleted = false)
        {
            var result = await _forumService.GetCommentRepliesAsync(commentId, cursor, pageSize, TryGetCurrentUserId(), includeDeleted);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UpsertForumPostRequest request)
        {
            try
            {
                var created = await _forumService.CreatePostAsync(
                    GetCurrentUserId(),
                    request.Title,
                    request.Description,
                    request.AttachedGalleryItemId,
                    request.Tags);
                return Ok(created);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpsertForumPostRequest request)
        {
            var success = await _forumService.UpdatePostAsync(
                id,
                GetCurrentUserId(),
                request.Title,
                request.Description,
                request.AttachedGalleryItemId,
                request.Tags);

            return success ? Ok() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _forumService.DeletePostAsync(id, GetCurrentUserId());
            return success ? Ok() : NotFound();
        }

        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            var success = await _forumService.RestorePostAsync(id, GetCurrentUserId());
            return success ? Ok() : NotFound();
        }

        [HttpPatch("{id}/pin")]
        public async Task<IActionResult> SetPinnedState(int id, [FromQuery] bool isPinned)
        {
            var success = await _forumService.SetPinnedStateAsync(id, GetCurrentUserId(), isPinned);
            return success ? Ok() : NotFound();
        }

        [HttpPatch("{id}/lock")]
        public async Task<IActionResult> SetLockedState(int id, [FromBody] LockStateRequest request)
        {
            var success = await _forumService.SetLockedStateAsync(id, GetCurrentUserId(), request.IsLocked, request.Reason);
            return success ? Ok() : NotFound();
        }

        [HttpPost("{id}/react")]
        public async Task<IActionResult> TogglePostReaction(int id, [FromQuery] bool isLike)
        {
            var success = await _forumService.TogglePostReactionAsync(id, GetCurrentUserId(), isLike);
            return success ? Ok() : NotFound();
        }

        [HttpPost("{id}/comment")]
        public async Task<IActionResult> AddComment(int id, [FromBody] AddCommentRequest request)
        {
            try
            {
                var created = await _forumService.AddCommentAsync(id, GetCurrentUserId(), request.Message, request.ParentCommentId);
                return Ok(created);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("comment/{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var success = await _forumService.DeleteCommentAsync(commentId, GetCurrentUserId());
            return success ? Ok() : NotFound();
        }

        [HttpPatch("comment/{commentId}/restore")]
        public async Task<IActionResult> RestoreComment(int commentId)
        {
            var success = await _forumService.RestoreCommentAsync(commentId, GetCurrentUserId());
            return success ? Ok() : NotFound();
        }

        [HttpPost("comment/{commentId}/react")]
        public async Task<IActionResult> ToggleCommentReaction(int commentId, [FromQuery] bool isLike)
        {
            var success = await _forumService.ToggleCommentReactionAsync(commentId, GetCurrentUserId(), isLike);
            return success ? Ok() : NotFound();
        }
    }
}
