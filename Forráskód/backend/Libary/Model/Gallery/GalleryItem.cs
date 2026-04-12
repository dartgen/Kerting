using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Libary.Model.Gallery
{
    /// <summary>
    /// Galéria kép/feltöltés fő entitás komment és reakció kapcsolatokkal.
    /// </summary>
    public class GalleryItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string FileExtension { get; set; } = ".jpg";
        public bool IsPublished { get; set; } = true;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        public int? DeletedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        [ForeignKey("UserId")]
        public Auth.Login Login { get; set; } = null!;
        public ICollection<GalleryComment> Comments { get; set; } = new List<GalleryComment>();
        public ICollection<GalleryReaction> Reactions { get; set; } = new List<GalleryReaction>();
    }
}
