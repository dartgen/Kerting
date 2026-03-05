using Microsoft.EntityFrameworkCore;
using Libary.Model.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary
{
    public class KertingDbContext : DbContext
    {
        public DbSet<Login> Login { get; set; }
        public KertingDbContext(DbContextOptions options) : base(options)
        {
        }

        protected KertingDbContext()
        {
        }
    }
}
