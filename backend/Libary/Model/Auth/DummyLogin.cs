using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model.Auth
{
    /// <summary>
    /// Egyszerű bejelentkezési tesztmodell (felhasználónév + jelszó).
    /// </summary>
    public class DummyLogin
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
