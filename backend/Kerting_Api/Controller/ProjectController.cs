using Libary.Model.Project;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Kerting_Api.DTO;
using Kerting_Api.Interface;

namespace Kerting_Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        // Segédfüggvény: Kinyeri a bejelentkezett user ID-ját a tokenből
        private string GetCurrentUserId()
        {
            // A JWT tokenedben lévő "Id" claim-et olvassuk ki, így biztosan a számszerű ID-t (pl. "10") kapjuk!
            return User.FindFirst("Id")?.Value ?? "";
        }

        // GET: api/project
        [HttpGet]
        public async Task<IActionResult> GetMyProjects()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var projects = await _projectService.GetUserProjectsAsync(userId);
            return Ok(projects);
        }

        // POST: api/project
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] ProjectDto projectDto)
        {
            var userId = GetCurrentUserId();
            var newProject = await _projectService.CreateProjectAsync(userId, projectDto);
            return Ok(newProject);
        }

        // PUT: api/project/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] ProjectDto projectDto)
        {
            var updatedProject = await _projectService.UpdateProjectAsync(id, projectDto);
            if (updatedProject == null) return NotFound();

            return Ok(updatedProject);
        }

        // DELETE: api/project/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userId = GetCurrentUserId();
            await _projectService.DeleteProjectAsync(id, userId);
            return NoContent(); // 204 No Content (sikeres törlés)
        }

        // --- FELADATOK (TASKS) VÉGPONTJAI ---

        // POST: api/project/5/tasks
        [HttpPost("{projectId}/tasks")]
        public async Task<IActionResult> CreateTask(int projectId, [FromBody] TaskDto taskDto)
        {
            var savedTask = await _projectService.SaveTaskAsync(projectId, taskDto);
            return Ok(savedTask);
        }

        // PUT: api/project/5/tasks/10
        [HttpPut("{projectId}/tasks/{taskId}")]
        public async Task<IActionResult> UpdateTask(int projectId, int taskId, [FromBody] TaskDto taskDto)
        {
            taskDto.Id = taskId; // Biztosítjuk, hogy az ID egyezzen
            var savedTask = await _projectService.SaveTaskAsync(projectId, taskDto);
            return Ok(savedTask);
        }

        // DELETE: api/project/5/tasks/10
        [HttpDelete("{projectId}/tasks/{taskId}")]
        public async Task<IActionResult> DeleteTask(int projectId, int taskId)
        {
            await _projectService.DeleteTaskAsync(projectId, taskId);
            return NoContent();
        }

        // ==========================================
        // --- MEGHÍVÓK KEZELÉSE (EZ HIÁNYZOTT!) ---
        // ==========================================

        // POST: api/Project/5/invite
        [HttpPost("{projectId}/invite")]
        public async Task<IActionResult> InviteMember(int projectId, [FromBody] string userIdToInvite)
        {
            // Eltávolítjuk a felesleges idézőjeleket a JSON stringből (mivel az Axios sima stringként küldi)
            var cleanUserId = userIdToInvite.Replace("\"", "");
            await _projectService.InviteMemberAsync(projectId, cleanUserId);
            return Ok();
        }

        // POST: api/Project/5/accept
        [HttpPost("{projectId}/accept")]
        public async Task<IActionResult> AcceptInvite(int projectId)
        {
            var userId = GetCurrentUserId();
            await _projectService.AcceptInviteAsync(projectId, userId);
            return Ok();
        }

        // POST: api/Project/5/reject
        [HttpPost("{projectId}/reject")]
        public async Task<IActionResult> RejectInvite(int projectId)
        {
            var userId = GetCurrentUserId();
            await _projectService.RejectInviteAsync(projectId, userId);
            return Ok();
        }
    }
}