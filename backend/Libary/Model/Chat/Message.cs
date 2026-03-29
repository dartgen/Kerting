using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model.Chat
{
    public class Message
    {
        public long Id { get; set; }
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }

        public int SenderId { get; set; }
        public User.User Sender { get; set; }

        public string Content { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
