using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model.Chat
{
    /// <summary>
    /// Új chat üzenet küldési DTO a kliens felől.
    /// </summary>
    public class SendMessageDto
    {
        public int ConversationId { get; set; }
        public string Content { get; set; }
    }
}
