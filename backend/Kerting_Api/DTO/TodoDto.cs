using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model.Project
{
    public class TodoDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public decimal? Amount { get; set; }
        public bool Completed { get; set; }
        public string? WorkerId { get; set; }
    }
}
