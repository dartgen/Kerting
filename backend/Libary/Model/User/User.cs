using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Libary.Model.User
{
    public class User
    {
        public int Id { get; set; }
        public string? VezetekNev { get; set; }
        public string? KeresztNev { get; set; }
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public string? Telepules { get; set; }
        public int RoleId { get; set; }
        public string? IMGString { get; set; }
        public string? Rolam { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? Tiktok { get; set; }

        [NotMapped]
        public List<string>? Cimkek { get; set; }
    }
}
