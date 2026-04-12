using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Libary.Model.Forum
{
    /// <summary>
    /// Fórum bejegyzéshez tartozó like/dislike reakció entitás.
    /// </summary>
    public class ForumPostReaction
    {
        public int Id { get; set; }
        public int ForumPostId { get; set; }
        public int UserId { get; set; }
        public bool IsLike { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        [ForeignKey("ForumPostId")]
        public ForumPost ForumPost { get; set; } = null!;

        [ForeignKey("UserId")]
        public Auth.Login Login { get; set; } = null!;
    }
}
