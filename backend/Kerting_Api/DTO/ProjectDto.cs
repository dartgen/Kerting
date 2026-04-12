using Libary.Model.Project;
using System.Collections.Generic;

namespace Kerting_Api.DTO
{
    public class ProjectDto
    {
        public int? Id { get; set; } // <--- KÉRDŐJEL IDE!
        public string? OwnerId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Deadline { get; set; }
        public string? Status { get; set; }
        public int? ChatConversationId { get; set; }
        public List<ProjectMemberDto>? Members { get; set; } = new();
        public List<TaskDto>? Tasks { get; set; } = new();
    }
}