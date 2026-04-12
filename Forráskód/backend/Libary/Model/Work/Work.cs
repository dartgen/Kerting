using Libary.Model.User;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Libary.Model.Work
{
    [Table("Work")]
    /// <summary>
    /// Work fő entitás: hirdetett munka, hozzá tartozó jelentkezők, teendők, képek és címkék.
    /// </summary>
    public class Work
    {
        [Key]
        public int Id { get; set; }

        public int AuthorId { get; set; }
        [ForeignKey(nameof(AuthorId))]
        public User.User? Author { get; set; }

        [Required]
        [MaxLength(50)]
        public string TargetAudience { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? BasePrice { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Open";

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        public ICollection<WorkApplicant>? Applicants { get; set; } = new List<WorkApplicant>();
        public ICollection<WorkTodo>? Todos { get; set; } = new List<WorkTodo>();
        public ICollection<WorkImage>? Images { get; set; } = new List<WorkImage>();
        public ICollection<WorkTag>? Tags { get; set; } = new List<WorkTag>();

        // API kompatibilitás miatt használt segédmező: címkék egyszerű string listában.
        [NotMapped]
        public List<string>? Cimkek { get; set; }
    }
}