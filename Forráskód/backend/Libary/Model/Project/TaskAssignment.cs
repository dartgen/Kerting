using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model.Project
{
    public class TaskAssignment
    {
        public int Id { get; set; }
        public int ProjectTaskId { get; set; }
        public ProjectTask ProjectTask { get; set; }

        public string UserId { get; set; } // Itt tároljuk, hogy ki van ráosztva a feladatra
    }
}
