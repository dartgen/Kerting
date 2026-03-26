using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Libary.Model.Gallery;

namespace Kerting_Api.Interface
{
    public interface IGalleryService
    {
        // Galéria
        Task<GalleryItem> UploadItemAsync(int userId, string title, string description, IFormFile file, string contentRootPath);
        Task<bool> DeleteItemAsync(int itemId, int userId, string contentRootPath);
        Task<bool> RestoreItemAsync(int itemId, int userId);
        Task<bool> UpdateItemAsync(int itemId, int userId, string title, string description);
        Task<bool> SetPublishStateAsync(int itemId, int userId, bool isPublished);
        Task<List<object>> GetGalleryFeedAsync(int page = 1, int pageSize = 20, int? currentUserId = null, bool includeDeleted = false);
        Task<List<object>> GetOwnGalleryFeedAsync(int ownerUserId, int page = 1, int pageSize = 20, int? currentUserId = null, bool includeDeleted = false);
        Task<object?> GetGalleryItemByIdAsync(int itemId, int? currentUserId = null, bool includeDeleted = false);

        // Profilkép
        Task<string> UploadProfileImageAsync(int userId, IFormFile file, string contentRootPath);
        Task<bool> DeleteProfileImageAsync(int userId, string contentRootPath);

        // Interakciók
        Task<GalleryComment> AddCommentAsync(int itemId, int userId, string message);
        Task<bool> DeleteCommentAsync(int commentId, int userId);
        Task<bool> RestoreCommentAsync(int commentId, int userId);
        Task<bool> ToggleReactionAsync(int itemId, int userId, bool isLike);
    }
}