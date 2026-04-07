using Libary.Model.User;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Libary.Model.Work
{
    [Table("WorkApplicant")]
    public class WorkApplicant
    {
        [Key]
        public int Id { get; set; }

        public int WorkId { get; set; }
        [ForeignKey(nameof(WorkId))]
        public Work? Work { get; set; }

        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User.User? User { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? OfferedPrice { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}