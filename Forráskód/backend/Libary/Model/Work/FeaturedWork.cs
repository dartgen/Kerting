using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Libary.Model.Work
{
    [Table("FeaturedWork")]
    /// <summary>
    /// Kiemelt munkák listájának kapcsoló entitása időbélyeggel.
    /// </summary>
    public class FeaturedWork
    {
        [Key]
        public int Id { get; set; }

        public int WorkId { get; set; }
        [ForeignKey(nameof(WorkId))]
        public Work? Work { get; set; }

        public DateTime FeaturedAtUtc { get; set; } = DateTime.UtcNow;
    }
}