using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Libary.Model.Work
{
    [Table("WorkImage")]
    /// <summary>
    /// Work-hoz feltöltött képek entitása, kiemelt és before/after párosítás támogatással.
    /// </summary>
    public class WorkImage
    {
        [Key]
        public int Id { get; set; }

        public int WorkId { get; set; }
        [ForeignKey(nameof(WorkId))]
        public Work? Work { get; set; }

        [Required]
        [MaxLength(250)]
        public string ImageUrl { get; set; }

        public bool IsShowcase { get; set; } = false;

        public int? RelatedImageId { get; set; }
        [ForeignKey(nameof(RelatedImageId))]
        public WorkImage? RelatedImage { get; set; }

        public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
    }
}