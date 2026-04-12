using System.Collections.Generic;
using System.Threading.Tasks;
using Kerting_Api.DTO;

namespace Kerting_Api.Interface
{
    /// <summary>
    /// Projekt service szerződés: projekt/feladat kezelés és meghívók workflow.
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// User projektjeinek listázása.
        /// </summary>
        Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(string userId);

        /// <summary>
        /// Új projekt létrehozása.
        /// </summary>
        Task<ProjectDto> CreateProjectAsync(string userId, ProjectDto dto);

        /// <summary>
        /// Meglévő projekt frissítése.
        /// </summary>
        Task<ProjectDto> UpdateProjectAsync(int projectId, ProjectDto dto);

        /// <summary>
        /// Projekt törlése.
        /// </summary>
        Task DeleteProjectAsync(int projectId, string userId);

        /// <summary>
        /// User meghívása projektbe.
        /// </summary>
        Task InviteMemberAsync(int projectId, string userIdToInvite);

        /// <summary>
        /// Meghívó elfogadása.
        /// </summary>
        Task AcceptInviteAsync(int projectId, string userId);

        /// <summary>
        /// Meghívó visszautasítása.
        /// </summary>
        Task RejectInviteAsync(int projectId, string userId);

        /// <summary>
        /// Feladat létrehozása/frissítése projekten belül.
        /// </summary>
        Task<TaskDto> SaveTaskAsync(int projectId, TaskDto taskDto);

        /// <summary>
        /// Feladat törlése.
        /// </summary>
        Task DeleteTaskAsync(int projectId, int taskId);
    }
}