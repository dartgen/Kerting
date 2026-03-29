using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Libary;
using Libary.Model.Gallery;
using Kerting_Api.Interface;
using Microsoft.IdentityModel.Tokens;

namespace Kerting_Api.Service
{
    public class GalleryService : IGalleryService
    {
        private readonly KertingDbContext _context;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public GalleryService(KertingDbContext context)
        {
            _context = context;
        }

        // --- GALÉRIA LOGIKA ---

        public async Task<GalleryItem> UploadItemAsync(int userId, string title, string description, IFormFile file, string contentRootPath)
        {
            var extension = ValidateFile(file);

            var item = new GalleryItem
            {
                UserId = userId,
                Title = title,
                Description = description,
                FileExtension = extension,
                IsPublished = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.GalleryItem.Add(item);
            await _context.SaveChangesAsync();

            string folder = Path.Combine(contentRootPath, "Resources", "Gallery");
            await SaveFileAsync(file, folder, $"{item.Id}{extension}");

            return item;
        }

        public async Task<bool> DeleteItemAsync(int itemId, int userId, string contentRootPath)
        {
            var item = await _context.GalleryItem.FirstOrDefaultAsync(x => x.Id == itemId);
            if (item == null) return false;

            var isAdmin = await IsAdminAsync(userId);
            if (!isAdmin && item.UserId != userId) return false;

            if (item.IsDeleted) return true;

            item.IsDeleted = true;
            item.DeletedAtUtc = DateTime.UtcNow;
            item.DeletedByUserId = userId;
            item.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreItemAsync(int itemId, int userId)
        {
            var isAdmin = await IsAdminAsync(userId);
            if (!isAdmin) return false;

            var item = await _context.GalleryItem.FirstOrDefaultAsync(x => x.Id == itemId);
            if (item == null || !item.IsDeleted) return false;

            item.IsDeleted = false;
            item.IsPublished = false;
            item.DeletedAtUtc = null;
            item.DeletedByUserId = null;
            item.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateItemAsync(int itemId, int userId, string title, string description)
        {
            var item = await _context.GalleryItem.FirstOrDefaultAsync(x => x.Id == itemId && !x.IsDeleted);
            if (item == null) return false;
            if (item.UserId != userId) return false;

            item.Title = (title ?? string.Empty).Trim();
            item.Description = description?.Trim();
            item.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetPublishStateAsync(int itemId, int userId, bool isPublished)
        {
            var item = await _context.GalleryItem.FirstOrDefaultAsync(x => x.Id == itemId && !x.IsDeleted);
            if (item == null) return false;

            // Publish/unpublish is owner-only by business requirement.
            if (item.UserId != userId) return false;

            item.IsPublished = isPublished;
            item.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // --- PROFILKÉP LOGIKA ---

        public async Task<string> UploadProfileImageAsync(int userId, IFormFile file, string contentRootPath)
        {
            var extension = ValidateFile(file);

            string folder = Path.Combine(contentRootPath, "Resources", "Profiles");
            // A fájlnév profile_{userId}, így ha újat tölt fel, a régi felülíródik
            string fileName = $"{userId}{extension}";

            await SaveFileAsync(file, folder, fileName);

            // 1. LÉPÉS: Lekérjük a felhasználót az adatbázisból (helyes async módon)
            var user = await _context.User.FindAsync(userId);

            // 2. LÉPÉS: Ellenőrizzük, hogy létezik-e (ne fagyjon le a kód, ha hibás az ID)
            if (user != null)
            {
                // 3. LÉPÉS: Frissítjük a memóriában lévő objektumot
                user.IMGString = fileName;

                // 4. LÉPÉS: Kimentjük a változást az adatbázisba (EZ HIÁNYZOTT!)
                await _context.SaveChangesAsync();
            }

            return (fileName);
        }

        public async Task<bool> DeleteProfileImageAsync(int userId, string contentRootPath)
        {
            // Mivel nem tudjuk az kiterjesztést pontosan, végignézzük a mappát
            string folder = Path.Combine(contentRootPath, "Resources", "Profiles");
            if (!Directory.Exists(folder)) return false;

            var files = Directory.GetFiles(folder, $"{userId}.*");
            foreach (var f in files) File.Delete(f);

            return true;
        }

        // --- SEGÉDMETÓDUSOK ---

        private async Task SaveFileAsync(IFormFile file, string folder, string fileName)
        {
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string fullPath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }

        private string ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("A fájl üres.");
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(ext)) throw new ArgumentException("Nem támogatott formátum.");
            return ext;
        }

        // --- LEKÉRDEZÉSEK ÉS INTERAKCIÓK (Frissített URL-ekkel) ---

        public async Task<List<object>> GetGalleryFeedAsync(int page = 1, int pageSize = 20, int? currentUserId = null, bool includeDeleted = false)
        {
            var isAdmin = currentUserId.HasValue && await IsAdminAsync(currentUserId.Value);
            var canSeeDeleted = includeDeleted && isAdmin;

            return await _context.GalleryItem
                .Include(i => i.Login)
                .Where(i => i.IsPublished)
                .Where(i => canSeeDeleted || !i.IsDeleted)
                .OrderByDescending(i => i.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Join(
                    _context.User,
                    i => i.UserId,
                    u => u.Id,
                    (i,u) => new
                    {
                        i.Id,
                        i.UserId,
                        i.Title,
                        i.Description,
                        i.IsPublished,
                        i.IsDeleted,
                        ImageUrl = $"/resources/Gallery/{i.Id}{i.FileExtension}",
                        UploaderName = u.VezetekNev.IsNullOrEmpty() || u.KeresztNev.IsNullOrEmpty() ? i.Login.Username : $"{u.VezetekNev} {u.KeresztNev}",
                        ProfileImageUrl = string.IsNullOrWhiteSpace(u.IMGString)
                            ? null
                            : $"/resources/Profiles/{u.IMGString}",
                        i.CreatedAtUtc,
                        LikesCount = i.Reactions.Count(r => r.IsLike),
                        DislikesCount = i.Reactions.Count(r => !r.IsLike),
                        CommentsCount = i.Comments.Count(c => canSeeDeleted || !c.IsDeleted),
                        CanEdit = currentUserId.HasValue && i.UserId == currentUserId.Value,
                        CanDelete = currentUserId.HasValue && (i.UserId == currentUserId.Value || isAdmin),
                        CanPublishToggle = currentUserId.HasValue && i.UserId == currentUserId.Value
                    })
                .ToListAsync<object>();
        }

        public async Task<List<object>> GetOwnGalleryFeedAsync(int ownerUserId, int page = 1, int pageSize = 20, int? currentUserId = null, bool includeDeleted = false)
        {
            if (!currentUserId.HasValue) return new List<object>();

            var isAdmin = await IsAdminAsync(currentUserId.Value);
            var canAccess = isAdmin || currentUserId.Value == ownerUserId;
            if (!canAccess) return new List<object>();

            var canSeeDeleted = includeDeleted && isAdmin;

            return await _context.GalleryItem
                .Include(i => i.Login)
                .Where(i => i.UserId == ownerUserId)
                .Where(i => canSeeDeleted || !i.IsDeleted)
                .OrderByDescending(i => i.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Join(
                    _context.User,
                    i => i.UserId,
                    u => u.Id,
                    (i, u) => new
                    {
                        i.Id,
                        i.UserId,
                        i.Title,
                        i.Description,
                        i.IsPublished,
                        i.IsDeleted,
                        ImageUrl = $"/resources/Gallery/{i.Id}{i.FileExtension}",
                        UploaderName = i.Login.Username,
                        ProfileImageUrl = string.IsNullOrWhiteSpace(u.IMGString)
                            ? null
                            : $"/resources/Profiles/{u.IMGString}",
                        i.CreatedAtUtc,
                        LikesCount = i.Reactions.Count(r => r.IsLike),
                        DislikesCount = i.Reactions.Count(r => !r.IsLike),
                        CommentsCount = i.Comments.Count(c => canSeeDeleted || !c.IsDeleted),
                        CanEdit = currentUserId.HasValue && i.UserId == currentUserId.Value,
                        CanDelete = currentUserId.HasValue && (i.UserId == currentUserId.Value || isAdmin),
                        CanPublishToggle = currentUserId.HasValue && i.UserId == currentUserId.Value
                    })
                .ToListAsync<object>();
        }

        public async Task<List<object>> GetUserGalleryFeedAsync(int userId, int page = 1, int pageSize = 20, int? currentUserId = null, bool includeDeleted = false)
        {
            if (!currentUserId.HasValue) return new List<object>();

            var isAdmin = await IsAdminAsync(currentUserId.Value);
            var isOwner = currentUserId.Value == userId;
            var canSeeUnpublished = isAdmin || isOwner;
            var canSeeDeleted = includeDeleted && isAdmin;

            return await _context.GalleryItem
                .Include(i => i.Login)
                .Where(i => i.UserId == userId)
                .Where(i => canSeeDeleted || !i.IsDeleted)
                .Where(i => canSeeUnpublished || i.IsPublished)
                .OrderByDescending(i => i.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Join(
                    _context.User,
                    i => i.UserId,
                    u => u.Id,
                    (i, u) => new
                    {
                        i.Id,
                        i.UserId,
                        i.Title,
                        i.Description,
                        i.IsPublished,
                        i.IsDeleted,
                        ImageUrl = $"/resources/Gallery/{i.Id}{i.FileExtension}",
                        UploaderName = u.VezetekNev.IsNullOrEmpty() || u.KeresztNev.IsNullOrEmpty() ? i.Login.Username : $"{u.VezetekNev} {u.KeresztNev}",
                        ProfileImageUrl = string.IsNullOrWhiteSpace(u.IMGString)
                            ? null
                            : $"/resources/Profiles/{u.IMGString}",
                        i.CreatedAtUtc,
                        LikesCount = i.Reactions.Count(r => r.IsLike),
                        DislikesCount = i.Reactions.Count(r => !r.IsLike),
                        CommentsCount = i.Comments.Count(c => canSeeDeleted || !c.IsDeleted),
                        CanEdit = i.UserId == currentUserId.Value,
                        CanDelete = i.UserId == currentUserId.Value || isAdmin,
                        CanPublishToggle = i.UserId == currentUserId.Value
                    })
                .ToListAsync<object>();
        }

        public async Task<object?> GetGalleryItemByIdAsync(int itemId, int? currentUserId = null, bool includeDeleted = false)
        {
            var item = await _context.GalleryItem
                .Include(i => i.Login)
                .Join(
                    _context.User,
                    i => i.UserId,
                    u => u.Id,
                    (i, u) => new { Item = i, User = u })
                .FirstOrDefaultAsync(x => x.Item.Id == itemId);

            if (item == null) return null;

            var isAdmin = currentUserId.HasValue && await IsAdminAsync(currentUserId.Value);
            var canSeeDeleted = includeDeleted && isAdmin;
            var canSeeUnpublished = currentUserId.HasValue && (currentUserId.Value == item.Item.UserId || isAdmin);

            if (item.Item.IsDeleted && !canSeeDeleted) return null;
            if (!item.Item.IsPublished && !canSeeUnpublished) return null;

            var comments = await _context.GalleryComment
                .Include(c => c.Login)
                .Where(c => c.GalleryItemId == itemId)
                .Where(c => canSeeDeleted || !c.IsDeleted)
                .Join(
                    _context.User,
                    c => c.UserId,
                    u => u.Id,
                    (c, u) => new
                    {
                        c.Id,
                        c.UserId,
                        c.Message,
                        c.IsDeleted,
                        UserName = u.VezetekNev.IsNullOrEmpty() || u.KeresztNev.IsNullOrEmpty() ? c.Login.Username : $"{u.VezetekNev} {u.KeresztNev}",
                        c.CreatedAtUtc,
                        CanDelete = currentUserId.HasValue &&
                            (currentUserId.Value == c.UserId || currentUserId.Value == item.Item.UserId || isAdmin),
                        ProfileImageUrl = string.IsNullOrWhiteSpace(u.IMGString)
                            ? null
                            : $"/resources/Profiles/{u.IMGString}"
                    })
                .OrderByDescending(c => c.CreatedAtUtc)
                .ToListAsync();

            var likesCount = await _context.GalleryReaction
                .CountAsync(r => r.GalleryItemId == itemId && r.IsLike);

            var dislikesCount = await _context.GalleryReaction
                .CountAsync(r => r.GalleryItemId == itemId && !r.IsLike);

            bool? myReaction = null;
            if (currentUserId.HasValue)
            {
                myReaction = await _context.GalleryReaction
                    .Where(r => r.GalleryItemId == itemId && r.UserId == currentUserId.Value)
                    .Select(r => (bool?)r.IsLike)
                    .FirstOrDefaultAsync();
            }

            return new
            {
                item.Item.Id,
                item.Item.UserId,
                item.Item.Title,
                item.Item.Description,
                item.Item.IsPublished,
                item.Item.IsDeleted,
                CanEdit = currentUserId.HasValue && currentUserId.Value == item.Item.UserId,
                CanDelete = currentUserId.HasValue && (currentUserId.Value == item.Item.UserId || isAdmin),
                CanPublishToggle = currentUserId.HasValue && currentUserId.Value == item.Item.UserId,
                ImageUrl = $"/resources/Gallery/{item.Item.Id}{item.Item.FileExtension}",
                UploaderName = item.Item.Login.Username,
                ProfileImageUrl = string.IsNullOrWhiteSpace(item.User.IMGString)
                    ? null
                    : $"/resources/Profiles/{item.User.IMGString}",
                LikesCount = likesCount,
                DislikesCount = dislikesCount,
                MyReaction = myReaction,
                Comments = comments
            };
        }

        public async Task<GalleryComment> AddCommentAsync(int itemId, int userId, string message)
        {
            var item = await _context.GalleryItem.FirstOrDefaultAsync(i => i.Id == itemId);
            if (item == null || item.IsDeleted) throw new ArgumentException("A bejegyzés nem található.");

            var isAdmin = await IsAdminAsync(userId);
            if (!item.IsPublished && item.UserId != userId && !isAdmin)
            {
                throw new UnauthorizedAccessException("Ehhez a bejegyzéshez nincs hozzáférésed.");
            }

            var comment = new GalleryComment { GalleryItemId = itemId, UserId = userId, Message = message, CreatedAtUtc = DateTime.UtcNow };
            _context.GalleryComment.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _context.GalleryComment
                .Include(c => c.GalleryItem)
                .FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null) return false;

            if (comment.IsDeleted) return true;

            var isAdmin = await IsAdminAsync(userId);
            var isCommentOwner = comment.UserId == userId;
            var isItemOwner = comment.GalleryItem.UserId == userId;

            if (!isCommentOwner && !isItemOwner && !isAdmin) return false;

            comment.IsDeleted = true;
            comment.DeletedAtUtc = DateTime.UtcNow;
            comment.DeletedByUserId = userId;
            comment.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreCommentAsync(int commentId, int userId)
        {
            var isAdmin = await IsAdminAsync(userId);
            if (!isAdmin) return false;

            var comment = await _context.GalleryComment.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null || !comment.IsDeleted) return false;

            comment.IsDeleted = false;
            comment.DeletedAtUtc = null;
            comment.DeletedByUserId = null;
            comment.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleReactionAsync(int itemId, int userId, bool isLike)
        {
            var item = await _context.GalleryItem.FirstOrDefaultAsync(i => i.Id == itemId);
            if (item == null || item.IsDeleted) return false;

            var isAdmin = await IsAdminAsync(userId);
            if (!item.IsPublished && item.UserId != userId && !isAdmin) return false;

            var reaction = await _context.GalleryReaction.FirstOrDefaultAsync(r => r.GalleryItemId == itemId && r.UserId == userId);
            if (reaction == null)
            {
                _context.GalleryReaction.Add(new GalleryReaction
                {
                    GalleryItemId = itemId,
                    UserId = userId,
                    IsLike = isLike,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
            else if (reaction.IsLike == isLike)
            {
                _context.GalleryReaction.Remove(reaction);
            }
            else
            {
                reaction.IsLike = isLike;
                reaction.CreatedAtUtc = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> IsAdminAsync(int userId)
        {
            return await _context.User
                .Where(u => u.Id == userId)
                .Select(u => u.RoleId == 1)
                .FirstOrDefaultAsync();
        }
    }
}