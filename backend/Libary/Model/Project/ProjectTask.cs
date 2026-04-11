using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model.Project
{
    public class ProjectTask
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public string Title { get; set; }
        public string? Description { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? Deadline { get; set; }
        public string Status { get; set; } = "todo"; // todo, in-progress, done

        public ICollection<TaskAssignment> AssignedTo { get; set; } = new List<TaskAssignment>();
        public ICollection<TodoItem> Todos { get; set; } = new List<TodoItem>();
    }
}
