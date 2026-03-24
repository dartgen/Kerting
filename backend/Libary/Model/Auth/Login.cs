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
        public bool FirstLogin { get; set; } = true;
    }
}
