using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model.Project
{
    public class Project
    {
        public int Id { get; set; }
        public string OwnerId { get; set; } // A bejelentkezett felhasználó Identity ID-ja
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public string Status { get; set; } = "ongoing"; // ongoing, archived, invited

        // Kapcsolatok
        public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
        public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    }
}
