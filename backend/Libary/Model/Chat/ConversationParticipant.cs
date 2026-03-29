using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model.Chat
{
    public class ConversationParticipant
    {
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }

        public int UserId { get; set; }
        public User.User User { get; set; } // Itt hivatkozunk a meglévő User modelledre

        public string Role { get; set; } = "Member";
        public DateTime JoinedAt { get; set; } = DateTime.Now;
    }
}
