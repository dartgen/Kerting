using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Libary.Model.Forum
{
    public class ForumComment
    {
        public int Id { get; set; }
        public int ForumPostId { get; set; }
        public int? ParentCommentId { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        public int? DeletedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        [ForeignKey("ForumPostId")]
        public ForumPost ForumPost { get; set; } = null!;

        [ForeignKey("ParentCommentId")]
        public ForumComment? ParentComment { get; set; }

        [ForeignKey("UserId")]
        public Auth.Login Login { get; set; } = null!;

        public ICollection<ForumComment> Replies { get; set; } = new List<ForumComment>();
        public ICollection<ForumCommentReaction> Reactions { get; set; } = new List<ForumCommentReaction>();
    }
}
