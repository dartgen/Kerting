using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Libary.Model.Gallery;

namespace Kerting_Api.Interface
{
    /// <summary>
    /// Galéria service szerződés: média CRUD, feedek, profilképkezelés és interakciók.
    /// </summary>
    public interface IGalleryService
    {
        /// <summary>
        /// Új galéria elem feltöltése.
        /// </summary>
        Task<GalleryItem> UploadItemAsync(int userId, string title, string description, IFormFile file, string contentRootPath);

        /// <summary>
        /// Galéria elem soft-delete.
        /// </summary>
        Task<bool> DeleteItemAsync(int itemId, int userId, string contentRootPath);

        /// <summary>
        /// Törölt galéria elem visszaállítása.
        /// </summary>
        Task<bool> RestoreItemAsync(int itemId, int userId);

        /// <summary>
        /// Galéria elem metadata frissítése.
        /// </summary>
        Task<bool> UpdateItemAsync(int itemId, int userId, string title, string description);

        /// <summary>
        /// Publikálási állapot váltás.
        /// </summary>
        Task<bool> SetPublishStateAsync(int itemId, int userId, bool isPublished);

        /// <summary>
        /// Publikus galéria feed lekérése.
        /// </summary>
        Task<List<object>> GetGalleryFeedAsync(int page = 1, int pageSize = 20, int? currentUserId = null, bool includeDeleted = false);

        /// <summary>
        /// Saját galéria feed lekérése.
        /// </summary>
        Task<List<object>> GetOwnGalleryFeedAsync(int ownerUserId, int page = 1, int pageSize = 20, int? currentUserId = null, bool includeDeleted = false);

        /// <summary>
        /// Egy user galéria feedje.
        /// </summary>
        Task<List<object>> GetUserGalleryFeedAsync(int userId, int page = 1, int pageSize = 20, int? currentUserId = null, bool includeDeleted = false);

        /// <summary>
        /// Galéria item részletes lekérése ID alapján.
        /// </summary>
        Task<object?> GetGalleryItemByIdAsync(int itemId, int? currentUserId = null, bool includeDeleted = false);

        /// <summary>
        /// Profilkép feltöltése.
        /// </summary>
        Task<string> UploadProfileImageAsync(int userId, IFormFile file, string contentRootPath);

        /// <summary>
        /// Profilkép törlése.
        /// </summary>
        Task<bool> DeleteProfileImageAsync(int userId, string contentRootPath);

        /// <summary>
        /// Új komment hozzáadása.
        /// </summary>
        Task<GalleryComment> AddCommentAsync(int itemId, int userId, string message);

        /// <summary>
        /// Komment soft-delete.
        /// </summary>
        Task<bool> DeleteCommentAsync(int commentId, int userId);

        /// <summary>
        /// Törölt komment visszaállítása.
        /// </summary>
        Task<bool> RestoreCommentAsync(int commentId, int userId);

        /// <summary>
        /// Like/dislike reakció váltás.
        /// </summary>
        Task<bool> ToggleReactionAsync(int itemId, int userId, bool isLike);
    }
}