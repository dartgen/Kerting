using Kerting_Api.Interface;
using Kerting_Api.DTO;
using Libary.Model.Work;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kerting_Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkController : ControllerBase
    {
        private readonly IWorkService _workService;

        public WorkController(IWorkService workService)
        {
            _workService = workService;
        }

        [HttpGet("open")]
        public async Task<IActionResult> GetOpenWorks(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 6,
            [FromQuery] decimal? priceMin = null,
            [FromQuery] decimal? priceMax = null,
            [FromQuery] DateTime? createdFrom = null,
            [FromQuery] DateTime? createdTo = null,
            [FromQuery] string? targetAudience = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var filters = new WorkFilterParams
                {
                    PriceMin = priceMin,
                    PriceMax = priceMax,
                    CreatedFrom = createdFrom,
                    CreatedTo = createdTo,
                    TargetAudience = targetAudience,
                    Status = status
                };

                var result = await _workService.GetAllOpenWorksAsync(page, pageSize, filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // If schema is not patched yet, keep frontend usable with an empty paginated response.
                if (ex.Message.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase))
                {
                    Response.Headers.Append("X-Work-Warning", "Work tables are missing. Run sql/work_patch.sql.");
                    return Ok(new PaginatedResponse<Work>(new List<Work>(), 0, 1, pageSize));
                }

                return StatusCode(500, new { message = "A munkák betöltése sikertelen.", detail = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWork(int id)
        {
            var work = await _workService.GetWorkByIdAsync(id);
            if (work == null) return NotFound();
            return Ok(work);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateWork([FromBody] Work work)
        {
            var userIdStr = User.FindFirstValue("Id");
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            try
            {
                work.AuthorId = int.Parse(userIdStr);
                var result = await _workService.CreateWorkAsync(work);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(503, new { message = "A Work adatbázis séma hiányzik. Futtasd a sql/work_patch.sql patch-et." });
                }

                return StatusCode(500, new { message = "Hiba történt a munka kiírásakor.", detail = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateWork(int id, [FromBody] Work work)
        {
            var userIdStr = User.FindFirstValue("Id");
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var existingWork = await _workService.GetWorkByIdAsync(id);
            if (existingWork == null) return NotFound();
            
            // Only Author or Admin can edit. We don't have Admin check here perfectly without role inject, but let's assume if it reaches here and not the author, return Forbid
            if (existingWork.AuthorId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var result = await _workService.UpdateWorkAsync(id, work);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteWork(int id)
        {
            var userIdStr = User.FindFirstValue("Id");
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var existingWork = await _workService.GetWorkByIdAsync(id);
            if (existingWork == null) return NotFound();

            if (existingWork.AuthorId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            await _workService.DeleteWorkAsync(id);
            return Ok();
        }

        [HttpPost("{id}/apply")]
        [Authorize]
        public async Task<IActionResult> ApplyForWork(int id, [FromBody] decimal? offeredPrice)
        {
            var userIdStr = User.FindFirstValue("Id");
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            try
            {
                var applicant = await _workService.ApplyForWorkAsync(id, int.Parse(userIdStr), offeredPrice);
                return Ok(applicant);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hiba történt a jelentkezés során.", detail = ex.Message });
            }
        }

        [HttpPost("applicant/{applicantId}/accept")]
        [Authorize]
        public async Task<IActionResult> AcceptApplicant(int applicantId)
        {
            var userIdStr = User.FindFirstValue("Id");
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var accepted = await _workService.AcceptApplicantAsync(applicantId);
            return Ok(accepted);
        }

        [HttpPost("{id}/todo")]
        [Authorize]
        public async Task<IActionResult> AddTodo(int id, [FromBody] WorkTodo todo)
        {
            var userIdStr = User.FindFirstValue("Id");
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var result = await _workService.AddTodoAsync(id, todo, int.Parse(userIdStr));
            return Ok(result);
        }

        [HttpPost("todo/{todoId}/toggle")]
        [Authorize]
        public async Task<IActionResult> ToggleTodo(int todoId, [FromBody] string doneMessage)
        {
            var userIdStr = User.FindFirstValue("Id");
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var result = await _workService.ToggleTodoAsync(todoId, int.Parse(userIdStr), doneMessage);
            return Ok(result);
        }

        [HttpPost("{id}/image")]
        [Authorize]
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded");
            
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Work");
            var img = await _workService.UploadWorkImageAsync(id, file, directoryPath);
            return Ok(img);
        }

        [HttpPost("image/{imageId}/showcase")]
        [Authorize(Roles = "Gardener,Admin,User")] // or logic based
        public async Task<IActionResult> ToggleShowcaseImage(int imageId)
        {
            var success = await _workService.ToggleShowcaseImageAsync(imageId);
            if (!success) return NotFound();
            return Ok();
        }

        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var result = await _workService.SetWorkStatusAsync(id, status);
            return Ok(result);
        }

        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedWorks()
        {
            var featured = await _workService.GetFeaturedWorksAsync();
            return Ok(featured);
        }

        [HttpPost("{id}/feature")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FeatureWork(int id)
        {
            var featured = await _workService.FeatureWorkAsync(id);
            if (featured == null) return BadRequest("Could not feature work (maybe it's not Public).");
            return Ok(featured);
        }

        [HttpDelete("featured/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveFeaturedWork(int id)
        {
            await _workService.RemoveFeaturedWorkAsync(id);
            return Ok();
        }

        // Phase 4: Bulk Image Upload
        [HttpPost("{id}/images")]
        [Authorize]
        public async Task<IActionResult> UploadImages(int id, IFormFileCollection files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded");

            try
            {
                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Work");
                var images = await _workService.UploadWorkImagesAsync(id, files, directoryPath);
                return Ok(images);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hiba a képek feltöltése során.", detail = ex.Message });
            }
        }

        // Phase 4: Delete Image
        [HttpDelete("{workId}/image/{imageId}")]
        [Authorize]
        public async Task<IActionResult> DeleteImage(int workId, int imageId)
        {
            var userIdStr = User.FindFirstValue("Id");
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            try
            {
                await _workService.DeleteWorkImageAsync(imageId, int.Parse(userIdStr));
                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hiba a kép törlése során.", detail = ex.Message });
            }
        }

        // Phase 4: Update Image Metadata
        [HttpPatch("{workId}/image/{imageId}")]
        [Authorize]
        public async Task<IActionResult> UpdateImageMetadata(int workId, int imageId, [FromBody] WorkImage metadata)
        {
            try
            {
                var result = await _workService.UpdateImageMetadataAsync(imageId, metadata);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hiba a kép metaadatainak frissítése során.", detail = ex.Message });
            }
        }

        // Phase 4: Link Image Pair
        [HttpPost("image/{imageId}/link")]
        [Authorize]
        public async Task<IActionResult> LinkImagePair(int imageId, [FromBody] int relatedImageId)
        {
            try
            {
                var result = await _workService.LinkImagePairAsync(imageId, relatedImageId);
                return Ok(new { success = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hiba a képek összekapcsolása során.", detail = ex.Message });
            }
        }

        // Phase 6: Reject Applicant
        [HttpPost("applicant/{applicantId}/reject")]
        [Authorize]
        public async Task<IActionResult> RejectApplicant(int applicantId)
        {
            var userIdStr = User.FindFirstValue("Id");
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            try
            {
                var result = await _workService.RejectApplicantAsync(applicantId, int.Parse(userIdStr));
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hiba az elutasítás során.", detail = ex.Message });
            }
        }

        // Phase 6: Withdraw Application
        [HttpPost("applicant/{applicantId}/withdraw")]
        [Authorize]
        public async Task<IActionResult> WithdrawApplication(int applicantId)
        {
            var userIdStr = User.FindFirstValue("Id");
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            try
            {
                var result = await _workService.WithdrawApplicationAsync(applicantId, int.Parse(userIdStr));
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hiba a visszavonás során.", detail = ex.Message });
            }
        }
    }
}
