using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model.Auth
{
    public class Role
    {
        // Kicseréljük a stringet egy kapcsolatra:
        public int Id { get; set; }
        public string Name { get; set; } // Pl: "Admin", "User", "Manager"
    }
}
