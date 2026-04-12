using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Libary.Model.Gallery
{
    /// <summary>
    /// Galéria elemhez tartozó komment entitás soft delete támogatással.
    /// </summary>
    public class GalleryComment
    {
        public int Id { get; set; }
        public int GalleryItemId { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        public int? DeletedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        [ForeignKey("GalleryItemId")]
        public GalleryItem GalleryItem { get; set; } = null!;
        
        [ForeignKey("UserId")]
        public Auth.Login Login { get; set; } = null!;
    }
}
