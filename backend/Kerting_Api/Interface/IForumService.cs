using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kerting_Api.Interface
{
    /// <summary>
    /// Fórum service szerződés: feed, post, komment, reakció és moderáció.
    /// </summary>
    public interface IForumService
    {
        /// <summary>
        /// Feed lekérése lapozással és szűrésekkel.
        /// </summary>
        Task<object> GetFeedAsync(
            int page,
            int pageSize,
            int? currentUserId,
            string? sort,
            string? search,
            int? maxAgeDays,
            List<int>? roleIds,
            List<string>? tagNames,
            bool includeDeleted);

        /// <summary>
        /// Egy post részletes adatai kommentlapozással.
        /// </summary>
        Task<object?> GetPostByIdAsync(
            int postId,
            int? currentUserId,
            bool includeDeleted,
            int commentCursor,
            int commentPageSize,
            int replyPageSize);

        /// <summary>
        /// Egy komment válaszainak lapozott listázása.
        /// </summary>
        Task<object> GetCommentRepliesAsync(
            int commentId,
            int cursor,
            int pageSize,
            int? currentUserId,
            bool includeDeleted);

        /// <summary>
        /// Új fórum post létrehozása.
        /// </summary>
        Task<object> CreatePostAsync(
            int userId,
            string title,
            string description,
            int? attachedGalleryItemId,
            List<string>? tagNames);

        /// <summary>
        /// Meglévő fórum post frissítése.
        /// </summary>
        Task<bool> UpdatePostAsync(
            int postId,
            int userId,
            string title,
            string description,
            int? attachedGalleryItemId,
            List<string>? tagNames);

        /// <summary>
        /// Post soft-delete.
        /// </summary>
        Task<bool> DeletePostAsync(int postId, int userId);

        /// <summary>
        /// Törölt post visszaállítása.
        /// </summary>
        Task<bool> RestorePostAsync(int postId, int userId);

        /// <summary>
        /// Pin állapot módosítása.
        /// </summary>
        Task<bool> SetPinnedStateAsync(int postId, int userId, bool isPinned);

        /// <summary>
        /// Lock állapot módosítása.
        /// </summary>
        Task<bool> SetLockedStateAsync(int postId, int userId, bool isLocked, string? reason);

        /// <summary>
        /// Új komment vagy válasz létrehozása.
        /// </summary>
        Task<object> AddCommentAsync(int postId, int userId, string message, int? parentCommentId);

        /// <summary>
        /// Komment soft-delete.
        /// </summary>
        Task<bool> DeleteCommentAsync(int commentId, int userId);

        /// <summary>
        /// Törölt komment visszaállítása.
        /// </summary>
        Task<bool> RestoreCommentAsync(int commentId, int userId);

        /// <summary>
        /// Post like/dislike reakció váltás.
        /// </summary>
        Task<bool> TogglePostReactionAsync(int postId, int userId, bool isLike);

        /// <summary>
        /// Komment like/dislike reakció váltás.
        /// </summary>
        Task<bool> ToggleCommentReactionAsync(int commentId, int userId, bool isLike);
    }
}
