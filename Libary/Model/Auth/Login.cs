using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace Libary.Model.Auth
{
    public class Login
    {   
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        // 1. A "Kulcs" (ennek egyeznie kell az SQL oszlop nevével!)
        public int RoleId { get; set; }

        // 2. A "Kapcsolat" (ez csak a kódnak kell, az SQL nem látja)
        // A [ForeignKey] attribútummal "biztosra megyünk", megmondjuk neki, melyik mező a kulcs.
        [ForeignKey("RoleId")]
        public Role Role { get; set; }
    }
}
