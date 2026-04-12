using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Libary.Model.Chat
{
    /// <summary>
    /// Chat kép feltöltési kérésmodell: beszélgetés azonosító és a feltöltendő fájl.
    /// </summary>
    public class ImageUploadDto
    {
        public int ConversationId { get; set; }
        public IFormFile Image { get; set; }
    }
}
