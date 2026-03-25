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
using Microsoft.AspNetCore.Http.HttpResults;

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
            var item = await _context.GalleryItem.FirstOrDefaultAsync(x => x.Id == itemId && x.UserId == userId);
            if (item == null) return false;

            string filePath = Path.Combine(contentRootPath, "Resources", "Gallery", $"{item.Id}{item.FileExtension}");
            if (File.Exists(filePath)) File.Delete(filePath);

            _context.GalleryItem.Remove(item);
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

            var files = Directory.GetFiles(folder, $"profile_{userId}.*");
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

        public async Task<List<object>> GetGalleryFeedAsync(int page = 1, int pageSize = 20)
        {
            return await _context.GalleryItem
                .Include(i => i.Login)
                .OrderByDescending(i => i.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new {
                    i.Id,
                    i.Title,
                    i.Description,
                    ImageUrl = $"/resources/gallery/{i.Id}{i.FileExtension}",
                    UploaderName = i.Login.Username,
                    i.CreatedAtUtc,
                    LikesCount = i.Reactions.Count(r => r.IsLike),
                    CommentsCount = i.Comments.Count()
                }).ToListAsync<object>();
        }

        public async Task<object?> GetGalleryItemByIdAsync(int itemId)
        {
            var item = await _context.GalleryItem
                .Include(i => i.Login)
                .Include(i => i.Comments).ThenInclude(c => c.Login)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null) return null;

            return new
            {
                item.Id,
                item.Title,
                item.Description,
                ImageUrl = $"/resources/gallery/{item.Id}{item.FileExtension}",
                UploaderName = item.Login.Username,
                Comments = item.Comments.Select(c => new { c.Message, UserName = c.Login.Username })
            };
        }

        public async Task<GalleryComment> AddCommentAsync(int itemId, int userId, string message)
        {
            var comment = new GalleryComment { GalleryItemId = itemId, UserId = userId, Message = message, CreatedAtUtc = DateTime.UtcNow };
            _context.GalleryComment.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _context.GalleryComment.FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);
            if (comment == null) return false;
            _context.GalleryComment.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleReactionAsync(int itemId, int userId, bool isLike)
        {
            var reaction = await _context.GalleryReaction.FirstOrDefaultAsync(r => r.GalleryItemId == itemId && r.UserId == userId);
            if (reaction != null) _context.GalleryReaction.Remove(reaction);
            else _context.GalleryReaction.Add(new GalleryReaction { GalleryItemId = itemId, UserId = userId, IsLike = isLike, CreatedAtUtc = DateTime.UtcNow });

            await _context.SaveChangesAsync();
            return true;
        }
    }
}