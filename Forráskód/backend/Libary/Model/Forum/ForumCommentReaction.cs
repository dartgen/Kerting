using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Libary.Model.Forum
{
    /// <summary>
    /// Kommentre adott like/dislike reakció entitás.
    /// </summary>
    public class ForumCommentReaction
    {
        public int Id { get; set; }
        public int ForumCommentId { get; set; }
        public int UserId { get; set; }
        public bool IsLike { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        [ForeignKey("ForumCommentId")]
        public ForumComment ForumComment { get; set; } = null!;

        [ForeignKey("UserId")]
        public Auth.Login Login { get; set; } = null!;
    }
}
