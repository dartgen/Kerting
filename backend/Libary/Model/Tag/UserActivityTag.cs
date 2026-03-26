using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model.Tag
{
    public class UserActivityTag
    {
        [Key]
        public int USerId { get; set; }
        public int TagId { get; set; }
    }
}
