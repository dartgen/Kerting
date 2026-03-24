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

namespace Kerting_Api.Service
{
    public class GalleryService : IGalleryService
    {
        private readonly KertingDbContext _context;

        public GalleryService(KertingDbContext context)
        {
            _context = context;
        }

        public async Task<GalleryItem> UploadItemAsync(int userId, string title, string description, IFormFile file, string webRootPath)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty.");

            var extension = Path.GetExtension(file.FileName).ToLower();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp", ".avif" };
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file extension.");

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

            // Save directly to the backend 'Images' folder
            string uploadsFolder = Path.Combine(webRootPath, "Images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string fileName = $"{item.Id}{extension}";
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return item;
        }

        public async Task<bool> DeleteItemAsync(int itemId, int userId, string webRootPath)
        {
            var item = await _context.GalleryItem.FirstOrDefaultAsync(x => x.Id == itemId && x.UserId == userId);
            if (item == null) return false;

            _context.GalleryItem.Remove(item);
            await _context.SaveChangesAsync();

            string uploadsFolder = Path.Combine(webRootPath, "Images");
            string fileName = $"{item.Id}{item.FileExtension}";
            string filePath = Path.Combine(uploadsFolder, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return true;
        }

        public async Task<GalleryComment> AddCommentAsync(int itemId, int userId, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message cannot be empty.");

            var comment = new GalleryComment
            {
                GalleryItemId = itemId,
                UserId = userId,
                Message = message,
                CreatedAtUtc = DateTime.UtcNow
            };

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
            var existingReaction = await _context.GalleryReaction
                .FirstOrDefaultAsync(r => r.GalleryItemId == itemId && r.UserId == userId);

            if (existingReaction != null)
            {
                if (existingReaction.IsLike == isLike)
                {
                    _context.GalleryReaction.Remove(existingReaction);
                }
                else
                {
                    existingReaction.IsLike = isLike;
                    existingReaction.CreatedAtUtc = DateTime.UtcNow;
                    _context.GalleryReaction.Update(existingReaction);
                }
            }
            else
            {
                var reaction = new GalleryReaction
                {
                    GalleryItemId = itemId,
                    UserId = userId,
                    IsLike = isLike,
                    CreatedAtUtc = DateTime.UtcNow
                };
                _context.GalleryReaction.Add(reaction);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<object>> GetGalleryFeedAsync(int page = 1, int pageSize = 20)
        {
            // Simplified return due to not having navigation properties back from User
            var items = await _context.GalleryItem
                .Include(i => i.Login)
                .Include(i => i.Reactions)
                .Include(i => i.Comments)
                .OrderByDescending(i => i.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new
                {
                    i.Id,
                    i.Title,
                    i.Description,
                    ImageUrl = $"/Images/{i.Id}{i.FileExtension}",
                    UploaderId = i.UserId,
                    UploaderName = i.Login.Username,
                    i.CreatedAtUtc,
                    LikesCount = i.Reactions.Count(r => r.IsLike),
                    DislikesCount = i.Reactions.Count(r => !r.IsLike),
                    CommentsCount = i.Comments.Count()
                })
                .ToListAsync();

            return items.Cast<object>().ToList();
        }

        public async Task<object?> GetGalleryItemByIdAsync(int itemId)
        {
            var item = await _context.GalleryItem
                .Include(i => i.Login)
                .Include(i => i.Reactions)
                .Include(i => i.Comments)
                    .ThenInclude(c => c.Login)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null) return null;

            return new
            {
                item.Id,
                item.Title,
                item.Description,
                ImageUrl = $"/Images/{item.Id}{item.FileExtension}",
                UploaderId = item.UserId,
                UploaderName = item.Login.Username,
                item.CreatedAtUtc,
                LikesCount = item.Reactions.Count(r => r.IsLike),
                DislikesCount = item.Reactions.Count(r => !r.IsLike),
                Comments = item.Comments.OrderBy(c => c.CreatedAtUtc).Select(c => new
                {
                    c.Id,
                    c.Message,
                    c.CreatedAtUtc,
                    UserId = c.UserId,
                    UserName = c.Login.Username
                })
            };
        }
    }
}