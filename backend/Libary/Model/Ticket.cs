using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Ezzel jelöljük, hogy a probléma meg lett-e oldva
        public bool IsResolved { get; set; } = false;

        // Összekötjük a felhasználóval, aki beküldte
        public int UserId { get; set; }
    }
}
