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
    /// <summary>
    /// Projekt végpontok: projekt CRUD, feladatkezelés és meghívók workflow.
    /// </summary>
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        // Segédfüggvény: bejelentkezett user ID kiolvasása tokenből.
        private string GetCurrentUserId()
        {
            return User.FindFirst("Id")?.Value ?? "";
        }

        /// <summary>
        /// Bejelentkezett user projektjeinek listázása.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyProjects()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var projects = await _projectService.GetUserProjectsAsync(userId);
            return Ok(projects);
        }

        /// <summary>
        /// Új projekt létrehozása.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] ProjectDto projectDto)
        {
            var userId = GetCurrentUserId();
            var newProject = await _projectService.CreateProjectAsync(userId, projectDto);
            return Ok(newProject);
        }

        /// <summary>
        /// Meglévő projekt frissítése.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] ProjectDto projectDto)
        {
            var updatedProject = await _projectService.UpdateProjectAsync(id, projectDto);
            if (updatedProject == null) return NotFound();

            return Ok(updatedProject);
        }

        /// <summary>
        /// Projekt törlése jogosultság-ellenőrzéssel.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userId = GetCurrentUserId();
            await _projectService.DeleteProjectAsync(id, userId);
            return NoContent();
        }

        /// <summary>
        /// Új feladat létrehozása projekten belül.
        /// </summary>
        [HttpPost("{projectId}/tasks")]
        public async Task<IActionResult> CreateTask(int projectId, [FromBody] TaskDto taskDto)
        {
            var savedTask = await _projectService.SaveTaskAsync(projectId, taskDto);
            return Ok(savedTask);
        }

        /// <summary>
        /// Meglévő feladat frissítése.
        /// </summary>
        [HttpPut("{projectId}/tasks/{taskId}")]
        public async Task<IActionResult> UpdateTask(int projectId, int taskId, [FromBody] TaskDto taskDto)
        {
            // Biztosítjuk, hogy az útvonalban kapott ID és a kérésadat ID egyezzen.
            taskDto.Id = taskId;
            var savedTask = await _projectService.SaveTaskAsync(projectId, taskDto);
            return Ok(savedTask);
        }

        /// <summary>
        /// Feladat törlése projektből.
        /// </summary>
        [HttpDelete("{projectId}/tasks/{taskId}")]
        public async Task<IActionResult> DeleteTask(int projectId, int taskId)
        {
            await _projectService.DeleteTaskAsync(projectId, taskId);
            return NoContent();
        }

        /// <summary>
        /// Tag meghívása projektbe.
        /// </summary>
        [HttpPost("{projectId}/invite")]
        public async Task<IActionResult> InviteMember(int projectId, [FromBody] string userIdToInvite)
        {
            // Az axios plain string kérésadata miatt levágjuk az esetleges idézőjeleket.
            var cleanUserId = userIdToInvite.Replace("\"", "");
            await _projectService.InviteMemberAsync(projectId, cleanUserId);
            return Ok();
        }

        /// <summary>
        /// Meghívó elfogadása.
        /// </summary>
        [HttpPost("{projectId}/accept")]
        public async Task<IActionResult> AcceptInvite(int projectId)
        {
            var userId = GetCurrentUserId();
            await _projectService.AcceptInviteAsync(projectId, userId);
            return Ok();
        }

        /// <summary>
        /// Meghívó visszautasítása.
        /// </summary>
        [HttpPost("{projectId}/reject")]
        public async Task<IActionResult> RejectInvite(int projectId)
        {
            var userId = GetCurrentUserId();
            await _projectService.RejectInviteAsync(projectId, userId);
            return Ok();
        }
    }
}