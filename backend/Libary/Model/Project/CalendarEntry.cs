using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model.Project
{
    public class CalendarEntry
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Aki létrehozta
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }
    }
}
