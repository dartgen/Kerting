using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Libary.Model.Gallery
{
    /// <summary>
    /// Galéria elem like/dislike reakció entitás.
    /// </summary>
    public class GalleryReaction
    {
        public int Id { get; set; }
        public int GalleryItemId { get; set; }
        public int UserId { get; set; }
        public bool IsLike { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        [ForeignKey("GalleryItemId")]
        public GalleryItem GalleryItem { get; set; } = null!;
        
        [ForeignKey("UserId")]
        public Auth.Login Login { get; set; } = null!;
    }
}
