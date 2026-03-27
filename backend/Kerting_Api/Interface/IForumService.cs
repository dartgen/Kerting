using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kerting_Api.Interface
{
    public interface IForumService
    {
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

        Task<object?> GetPostByIdAsync(
            int postId,
            int? currentUserId,
            bool includeDeleted,
            int commentCursor,
            int commentPageSize,
            int replyPageSize);

        Task<object> GetCommentRepliesAsync(
            int commentId,
            int cursor,
            int pageSize,
            int? currentUserId,
            bool includeDeleted);

        Task<object> CreatePostAsync(
            int userId,
            string title,
            string description,
            int? attachedGalleryItemId,
            List<string>? tagNames);

        Task<bool> UpdatePostAsync(
            int postId,
            int userId,
            string title,
            string description,
            int? attachedGalleryItemId,
            List<string>? tagNames);

        Task<bool> DeletePostAsync(int postId, int userId);
        Task<bool> RestorePostAsync(int postId, int userId);
        Task<bool> SetPinnedStateAsync(int postId, int userId, bool isPinned);
        Task<bool> SetLockedStateAsync(int postId, int userId, bool isLocked, string? reason);

        Task<object> AddCommentAsync(int postId, int userId, string message, int? parentCommentId);
        Task<bool> DeleteCommentAsync(int commentId, int userId);
        Task<bool> RestoreCommentAsync(int commentId, int userId);

        Task<bool> TogglePostReactionAsync(int postId, int userId, bool isLike);
        Task<bool> ToggleCommentReactionAsync(int commentId, int userId, bool isLike);
    }
}
