using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Libary.Model.Forum
{
    public class ForumPost
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? AttachedGalleryItemId { get; set; }
        public bool IsPinned { get; set; }
        public bool IsLocked { get; set; }
        public string? LockReason { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        public int? DeletedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public DateTime LastActivityAtUtc { get; set; }
        public int ViewCount { get; set; }

        [ForeignKey("UserId")]
        public Auth.Login Login { get; set; } = null!;

        [ForeignKey("AttachedGalleryItemId")]
        public Gallery.GalleryItem? AttachedGalleryItem { get; set; }

        public ICollection<ForumComment> Comments { get; set; } = new List<ForumComment>();
        public ICollection<ForumPostReaction> Reactions { get; set; } = new List<ForumPostReaction>();
        public ICollection<ForumPostTag> PostTags { get; set; } = new List<ForumPostTag>();
    }
}
