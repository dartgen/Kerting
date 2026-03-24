using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model.User
{
    public class User
    {
        public int Id { get; set; }
        public string VezetekNev { get; set; }
        public string KeresztNev { get; set; }
        public string Email { get; set; }
        public string Telepules { get; set; }
        public int RoleId { get; set; }
        public int ProfileIMGId { get; set; }
        public string Rolam { get; set; }
    }
}
