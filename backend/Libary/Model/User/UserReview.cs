using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model.User
{
    public class UserReview
    {
        [Key]
        public int Id { get; set; }

        public int TargetUserId { get; set; }
        public int AuthorUserId { get; set; }
        public int? ParentReviewId { get; set; }
        public byte? Rating { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        public int? DeletedByUserId { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        // Navigációs property-k (ha be vannak állítva az OnModelCreating-ben)
        [ForeignKey("TargetUserId")]
        public User? TargetUser { get; set; }

        [ForeignKey("AuthorUserId")]
        public User? AuthorUser { get; set; }

        public ICollection<UserReviewReaction> Reactions { get; set; } = new List<UserReviewReaction>();
    }
}
