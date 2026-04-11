using System.Collections.Generic;
using System.Threading.Tasks;
using Kerting_Api.DTO;

namespace Kerting_Api.Interface
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(string userId);
        Task<ProjectDto> CreateProjectAsync(string userId, ProjectDto dto);
        Task<ProjectDto> UpdateProjectAsync(int projectId, ProjectDto dto);
        Task DeleteProjectAsync(int projectId, string userId);

        // --- MEGHÍVÓK ---
        Task InviteMemberAsync(int projectId, string userIdToInvite);
        Task AcceptInviteAsync(int projectId, string userId);
        Task RejectInviteAsync(int projectId, string userId);

        // --- FELADATOK ---
        Task<TaskDto> SaveTaskAsync(int projectId, TaskDto taskDto);
        Task DeleteTaskAsync(int projectId, int taskId);
    }
}