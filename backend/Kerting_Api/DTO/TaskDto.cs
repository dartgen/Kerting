using Libary.Model.Project;
using System.Collections.Generic;

namespace Kerting_Api.DTO
{
    public class TaskDto
    {
        public int? Id { get; set; } // <--- KÉRDŐJEL IDE IS!
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? Amount { get; set; }
        public string? Deadline { get; set; }
        public string? Status { get; set; }
        public List<string>? AssignedTo { get; set; } = new();
        public List<TodoDto>? Todos { get; set; } = new();
    }
}