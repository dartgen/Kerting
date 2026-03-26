using Microsoft.EntityFrameworkCore;
using Libary.Model.Auth;
using Libary.Model.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Libary.Model.Tag;
using Libary.Model.Gallery;

namespace Libary
{
    public class KertingDbContext : DbContext
    {
        public DbSet<Login> Login { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<UserActivityTag> UserActivityTag { get; set; }
        public DbSet<ActivityTag> ActivityTag { get; set; }
        public DbSet<GalleryItem> GalleryItem { get; set; }
        public DbSet<GalleryComment> GalleryComment { get; set; }
        public DbSet<GalleryReaction> GalleryReaction { get; set; }

        public KertingDbContext(DbContextOptions options) : base(options)
        {
        }

        protected KertingDbContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GalleryItem>(entity =>
            {
                entity.ToTable("GalleryItem", "dbo", t =>
                {
                    t.HasCheckConstraint(
                        "CK_GalleryItem_FileExtension",
                        "LOWER([FileExtension]) IN (N'.jpg', N'.jpeg', N'.png', N'.webp', N'.gif', N'.bmp', N'.avif')");
                });
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(2000);
                entity.Property(x => x.FileExtension).HasMaxLength(10).IsRequired();
                entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(x => x.Login)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .HasConstraintName("FK_GalleryItem_Login_UserId")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(x => new { x.UserId, x.CreatedAtUtc })
                    .HasDatabaseName("IX_GalleryItem_UserId_CreatedAtUtc");
            });

            modelBuilder.Entity<GalleryComment>(entity =>
            {
                entity.ToTable("GalleryComment", "dbo");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Message).HasMaxLength(1000).IsRequired();
                entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(x => x.GalleryItem)
                    .WithMany(x => x.Comments)
                    .HasForeignKey(x => x.GalleryItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Login)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .HasConstraintName("FK_GalleryComment_Login_UserId")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(x => new { x.GalleryItemId, x.CreatedAtUtc })
                    .HasDatabaseName("IX_GalleryComment_GalleryItemId_CreatedAtUtc");
            });

            modelBuilder.Entity<UserActivityTag>()
            .HasKey(uat => new { uat.USerId, uat.TagId });

            modelBuilder.Entity<GalleryReaction>(entity =>
            {
                entity.ToTable("GalleryReaction", "dbo");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(x => x.GalleryItem)
                    .WithMany(x => x.Reactions)
                    .HasForeignKey(x => x.GalleryItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Login)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .HasConstraintName("FK_GalleryReaction_Login_UserId")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(x => new { x.GalleryItemId, x.UserId })
                    .IsUnique()
                    .HasDatabaseName("UX_GalleryReaction_GalleryItemId_UserId");
            });
        }
    }
}
