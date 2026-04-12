using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model.User
{
    /// <summary>
    /// Felhasználói értékelésre adott reakció (like/dislike) entitás.
    /// </summary>
    public class UserReviewReaction
    {
        [Key]
        public int Id { get; set; }

        public int UserReviewId { get; set; }
        public int UserId { get; set; }
        public bool IsLike { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        [ForeignKey("UserReviewId")]
        public UserReview? UserReview { get; set; }
    }
}
