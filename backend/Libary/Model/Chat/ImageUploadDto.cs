using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Libary.Model.Chat
{
    public class ImageUploadDto
    {
        public int ConversationId { get; set; }
        public IFormFile Image { get; set; }
    }
}
