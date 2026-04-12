using Libary.Model.User;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Libary.Model.Work
{
    [Table("WorkTodo")]
    /// <summary>
    /// Work részfeladat (TODO) entitás állapottal és lezárási metaadatokkal.
    /// </summary>
    public class WorkTodo
    {
        [Key]
        public int Id { get; set; }

        public int WorkId { get; set; }
        [ForeignKey(nameof(WorkId))]
        public Work? Work { get; set; }

        [Required]
        [MaxLength(250)]
        public string Title { get; set; }

        public bool IsDone { get; set; } = false;

        [MaxLength(500)]
        public string? DoneMessage { get; set; }

        public int? DoneByUserId { get; set; }
        [ForeignKey(nameof(DoneByUserId))]
        public User.User? DoneByUser { get; set; }
    }
}