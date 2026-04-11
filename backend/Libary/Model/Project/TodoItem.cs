using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model.Project
{
    public class TodoItem
    {
        public int Id { get; set; }
        public int ProjectTaskId { get; set; }
        public ProjectTask ProjectTask { get; set; }

        public string Text { get; set; }
        public decimal? Amount { get; set; }
        public bool Completed { get; set; }
        public string? WorkerId { get; set; } // Annak a usernek az ID-ja, aki elvállalta az adott pipát
    }
}
