using System;

namespace Kerting_Api.Model.Gallery
{
    /// <summary>
    /// Galéria komment létrehozás kérésmodell.
    /// </summary>
    public class CommentRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Galéria komment API válaszmodell.
    /// </summary>
    public class CommentResponse
    {
        public int Id { get; set; }
        public int GalleryItemId { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public bool IsDeleted { get; set; }
    }
}
