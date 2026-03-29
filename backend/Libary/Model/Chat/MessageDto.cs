using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model.Chat
{
    public class MessageDto
    {
        public long Id { get; set; }
        public string Szoveg { get; set; }
        public string Ido { get; set; }
        public bool Sajat { get; set; } // true, ha a bejelentkezett felhasználó küldte
        public string SenderName { get; set; } // Csoportos chatnél fontos
    }
}
