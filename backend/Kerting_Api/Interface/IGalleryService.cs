using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Libary.Model.Gallery;

namespace Kerting_Api.Interface
{
    public interface IGalleryService
    {
        Task<GalleryItem> UploadItemAsync(int userId, string title, string description, IFormFile file, string webRootPath);
        Task<bool> DeleteItemAsync(int itemId, int userId, string webRootPath);
        Task<GalleryComment> AddCommentAsync(int itemId, int userId, string message);
        Task<bool> DeleteCommentAsync(int commentId, int userId);
        Task<bool> ToggleReactionAsync(int itemId, int userId, bool isLike);
        Task<List<object>> GetGalleryFeedAsync(int page = 1, int pageSize = 20);
        Task<object?> GetGalleryItemByIdAsync(int itemId);
    }
}