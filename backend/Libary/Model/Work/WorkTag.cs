using System.ComponentModel.DataAnnotations.Schema;
using Libary.Model.Tag;

namespace Libary.Model.Work
{
    [Table("WorkTag")]
    /// <summary>
    /// Work és aktivitási címke közötti kapcsolótábla entitás.
    /// </summary>
    public class WorkTag
    {
        public int WorkId { get; set; }
        [ForeignKey(nameof(WorkId))]
        public Work? Work { get; set; }

        public int TagId { get; set; }
        [ForeignKey(nameof(TagId))]
        public ActivityTag? Tag { get; set; }
    }
}
