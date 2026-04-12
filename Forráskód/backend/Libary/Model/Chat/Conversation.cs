using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model.Chat
{
    public class Conversation
    {
        public int Id { get; set; }
        public bool IsGroup { get; set; }
        public string? Title { get; set; }
        public string? GroupImage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastMessageAt { get; set; } = DateTime.Now;

        // Navigációs property-k EF Core-hoz
        public ICollection<ConversationParticipant> Participants { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
