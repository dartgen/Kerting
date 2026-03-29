using Libary.Model.Auth;
using Libary.Model.Chat;
using Libary.Model.Forum;
using Libary.Model.Gallery;
using Libary.Model.Tag;
using Libary.Model.User;
using Microsoft.EntityFrameworkCore;
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
        public DbSet<Role> Role { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<FeaturedUserSlot> FeaturedUserSlot { get; set; }
        public DbSet<UserReview> UserReview { get; set; }
        public DbSet<UserReviewReaction> UserReviewReaction { get; set; }
        public DbSet<UserActivityTag> UserActivityTag { get; set; }
        public DbSet<ActivityTag> ActivityTag { get; set; }
        public DbSet<GalleryItem> GalleryItem { get; set; }
        public DbSet<GalleryComment> GalleryComment { get; set; }
        public DbSet<GalleryReaction> GalleryReaction { get; set; }
        public DbSet<ForumPost> ForumPost { get; set; }
        public DbSet<ForumComment> ForumComment { get; set; }
        public DbSet<ForumPostReaction> ForumPostReaction { get; set; }
        public DbSet<ForumCommentReaction> ForumCommentReaction { get; set; }
        public DbSet<ForumPostTag> ForumPostTag { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
        public DbSet<Message> Messages { get; set; }

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
                entity.Property(x => x.IsPublished).HasDefaultValue(true);
                entity.Property(x => x.IsDeleted).HasDefaultValue(false);
                entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(x => x.Login)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .HasConstraintName("FK_GalleryItem_Login_UserId")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(x => new { x.UserId, x.CreatedAtUtc })
                    .HasDatabaseName("IX_GalleryItem_UserId_CreatedAtUtc");

                entity.HasIndex(x => new { x.IsPublished, x.IsDeleted, x.CreatedAtUtc })
                    .HasDatabaseName("IX_GalleryItem_Published_Deleted_CreatedAtUtc");
            });

            modelBuilder.Entity<GalleryComment>(entity =>
            {
                entity.ToTable("GalleryComment", "dbo");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Message).HasMaxLength(1000).IsRequired();
                entity.Property(x => x.IsDeleted).HasDefaultValue(false);
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

                entity.HasIndex(x => new { x.GalleryItemId, x.IsDeleted, x.CreatedAtUtc })
                    .HasDatabaseName("IX_GalleryComment_GalleryItemId_IsDeleted_CreatedAtUtc");
            });

            modelBuilder.Entity<UserActivityTag>()
            .HasKey(uat => new { uat.USerId, uat.TagId });

            modelBuilder.Entity<FeaturedUserSlot>(entity =>
            {
                entity.ToTable("FeaturedUserSlot", "dbo");
                entity.HasKey(x => x.SlotNo);

                entity.Property(x => x.SlotNo).ValueGeneratedNever();
                entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .HasConstraintName("FK_FeaturedUserSlot_User_UserId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => x.UserId)
                    .IsUnique()
                    .HasDatabaseName("UX_FeaturedUserSlot_UserId");
            });

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

            modelBuilder.Entity<ForumPost>(entity =>
            {
                entity.ToTable("ForumPost", "dbo");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
                entity.Property(x => x.LockReason).HasMaxLength(300);
                entity.Property(x => x.IsDeleted).HasDefaultValue(false);
                entity.Property(x => x.IsPinned).HasDefaultValue(false);
                entity.Property(x => x.IsLocked).HasDefaultValue(false);
                entity.Property(x => x.ViewCount).HasDefaultValue(0);
                entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(x => x.LastActivityAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(x => x.Login)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .HasConstraintName("FK_ForumPost_Login_UserId")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.AttachedGalleryItem)
                    .WithMany()
                    .HasForeignKey(x => x.AttachedGalleryItemId)
                    .HasConstraintName("FK_ForumPost_GalleryItem_AttachedGalleryItemId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(x => new { x.IsDeleted, x.IsPinned, x.CreatedAtUtc })
                    .HasDatabaseName("IX_ForumPost_Deleted_Pinned_CreatedAtUtc");

                entity.HasIndex(x => new { x.IsDeleted, x.LastActivityAtUtc })
                    .HasDatabaseName("IX_ForumPost_Deleted_LastActivityAtUtc");
            });

            modelBuilder.Entity<ForumComment>(entity =>
            {
                entity.ToTable("ForumComment", "dbo");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Message).HasMaxLength(1000).IsRequired();
                entity.Property(x => x.IsDeleted).HasDefaultValue(false);
                entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(x => x.ForumPost)
                    .WithMany(x => x.Comments)
                    .HasForeignKey(x => x.ForumPostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.ParentComment)
                    .WithMany(x => x.Replies)
                    .HasForeignKey(x => x.ParentCommentId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.Login)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .HasConstraintName("FK_ForumComment_Login_UserId")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(x => new { x.ForumPostId, x.ParentCommentId, x.CreatedAtUtc })
                    .HasDatabaseName("IX_ForumComment_Post_Parent_CreatedAtUtc");

                entity.HasIndex(x => new { x.ForumPostId, x.IsDeleted, x.CreatedAtUtc })
                    .HasDatabaseName("IX_ForumComment_Post_Deleted_CreatedAtUtc");
            });

            modelBuilder.Entity<ForumPostReaction>(entity =>
            {
                entity.ToTable("ForumPostReaction", "dbo");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(x => x.ForumPost)
                    .WithMany(x => x.Reactions)
                    .HasForeignKey(x => x.ForumPostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Login)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .HasConstraintName("FK_ForumPostReaction_Login_UserId")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(x => new { x.ForumPostId, x.UserId })
                    .IsUnique()
                    .HasDatabaseName("UX_ForumPostReaction_ForumPostId_UserId");
            });

            modelBuilder.Entity<ForumCommentReaction>(entity =>
            {
                entity.ToTable("ForumCommentReaction", "dbo");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(x => x.ForumComment)
                    .WithMany(x => x.Reactions)
                    .HasForeignKey(x => x.ForumCommentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Login)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .HasConstraintName("FK_ForumCommentReaction_Login_UserId")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(x => new { x.ForumCommentId, x.UserId })
                    .IsUnique()
                    .HasDatabaseName("UX_ForumCommentReaction_ForumCommentId_UserId");
            });

            modelBuilder.Entity<ForumPostTag>(entity =>
            {
                entity.ToTable("ForumPostTag", "dbo");
                entity.HasKey(x => new { x.ForumPostId, x.TagId });

                entity.HasOne(x => x.ForumPost)
                    .WithMany(x => x.PostTags)
                    .HasForeignKey(x => x.ForumPostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.ActivityTag)
                    .WithMany()
                    .HasForeignKey(x => x.TagId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => x.TagId)
                    .HasDatabaseName("IX_ForumPostTag_TagId");
            });

            // Beállítjuk a ConversationParticipant összetett kulcsát
            modelBuilder.Entity<ConversationParticipant>()
                .HasKey(cp => new { cp.ConversationId, cp.UserId });

            // 2. A kaszkádolt törlés megakadályozása (Hogy ne kapj SQL Server hibát)
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany() 
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
