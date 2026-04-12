using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model.Chat
{
    public class ChatListItemDto
    {
        public int Id { get; set; }
        public string Nev { get; set; } // Csoportnév VAGY a partner neve
        public string UtolsoUzenet { get; set; }
        public string UtolsoIdo { get; set; }
        public bool Olvasatlan { get; set; }
        public string Avatar { get; set; }
        public bool IsGroup { get; set; }
    }
}
