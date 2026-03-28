using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kerting_Api.Interface;
using Libary;
using Libary.Model.Forum;
using Libary.Model.Gallery;
using Microsoft.EntityFrameworkCore;

namespace Kerting_Api.Service
{
    public class ForumService : IForumService
    {
        private const int MaxDepth = 3;
        private readonly KertingDbContext _context;

        public ForumService(KertingDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetFeedAsync(
            int page,
            int pageSize,
            int? currentUserId,
            string? sort,
            string? search,
            int? maxAgeDays,
            List<int>? roleIds,
            List<string>? tagNames,
            bool includeDeleted)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var isAdmin = currentUserId.HasValue && await IsAdminAsync(currentUserId.Value);
            var canSeeDeleted = includeDeleted && isAdmin;
            var normalizedSort = (sort ?? "latest").Trim().ToLowerInvariant();
            var normalizedSearch = (search ?? string.Empty).Trim();
            var normalizedTags = NormalizeTags(tagNames);
            var selectedRoleIds = roleIds?.Distinct().ToList() ?? new List<int>();
            var ageCutoff = maxAgeDays.HasValue ? DateTime.UtcNow.AddDays(-Math.Max(0, maxAgeDays.Value)) : (DateTime?)null;

            var query = _context.ForumPost
                .AsNoTracking()
                .Include(p => p.Reactions)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.ActivityTag)
                .Include(p => p.AttachedGalleryItem)
                .Where(p => canSeeDeleted || !p.IsDeleted)
                .Where(p => !ageCutoff.HasValue || p.CreatedAtUtc >= ageCutoff.Value);

            if (!string.IsNullOrWhiteSpace(normalizedSearch))
            {
                query = query.Where(p => p.Title.Contains(normalizedSearch) || p.Description.Contains(normalizedSearch));
            }

            var baseRows = await query
                .Join(
                    _context.User,
                    p => p.UserId,
                    u => u.Id,
                    (p, u) => new { Post = p, User = u })
                .GroupJoin(
                    _context.Role,
                    pu => pu.User.RoleId,
                    r => r.Id,
                    (pu, role) => new { pu.Post, pu.User, Role = role.FirstOrDefault() })
                .ToListAsync();

            if (selectedRoleIds.Count > 0)
            {
                baseRows = baseRows.Where(x => selectedRoleIds.Contains(x.User.RoleId)).ToList();
            }

            var feedRows = baseRows
                .Select(x =>
                {
                    var likes = x.Post.Reactions.Count(r => r.IsLike);
                    var dislikes = x.Post.Reactions.Count(r => !r.IsLike);
                    var netScore = likes - dislikes;
                    var postTags = x.Post.PostTags
                        .Select(pt => (pt.ActivityTag.Activity ?? string.Empty).Trim())
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var tagMatchCount = normalizedTags.Count == 0
                        ? 0
                        : postTags.Count(t => normalizedTags.Contains(t, StringComparer.OrdinalIgnoreCase));

                    return new
                    {
                        x.Post,
                        x.User,
                        RoleName = (x.Role?.Name ?? string.Empty).Trim(),
                        Likes = likes,
                        Dislikes = dislikes,
                        NetScore = netScore,
                        Tags = postTags,
                        TagMatchCount = tagMatchCount
                    };
                })
                .ToList();

            if (normalizedTags.Count > 0)
            {
                feedRows = feedRows.Where(x => x.TagMatchCount > 0).ToList();
            }

            feedRows = normalizedSort switch
            {
                "oldest" => feedRows
                    .OrderByDescending(x => x.Post.IsPinned)
                    .ThenByDescending(x => x.TagMatchCount)
                    .ThenBy(x => x.Post.CreatedAtUtc)
                    .ToList(),
                "netasc" => feedRows
                    .OrderByDescending(x => x.Post.IsPinned)
                    .ThenByDescending(x => x.TagMatchCount)
                    .ThenBy(x => x.NetScore)
                    .ThenByDescending(x => x.Post.CreatedAtUtc)
                    .ToList(),
                "netdesc" => feedRows
                    .OrderByDescending(x => x.Post.IsPinned)
                    .ThenByDescending(x => x.TagMatchCount)
                    .ThenByDescending(x => x.NetScore)
                    .ThenByDescending(x => x.Post.CreatedAtUtc)
                    .ToList(),
                _ => feedRows
                    .OrderByDescending(x => x.Post.IsPinned)
                    .ThenByDescending(x => x.TagMatchCount)
                    .ThenByDescending(x => x.Post.CreatedAtUtc)
                    .ToList()
            };

            var paged = feedRows.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new
            {
                Page = page,
                PageSize = pageSize,
                HasMore = feedRows.Count > page * pageSize,
                Items = paged.Select(x => new
                {
                    x.Post.Id,
                    x.Post.UserId,
                    x.Post.Title,
                    x.Post.Description,
                    x.Post.CreatedAtUtc,
                    x.Post.UpdatedAtUtc,
                    x.Post.LastActivityAtUtc,
                    x.Post.IsDeleted,
                    x.Post.IsPinned,
                    x.Post.IsLocked,
                    x.Post.LockReason,
                    x.Post.ViewCount,
                    AuthorName = BuildUserDisplayName(x.User, "Felhasználó"),
                    AuthorRoleName = x.RoleName,
                    ProfileImageUrl = string.IsNullOrWhiteSpace(x.User.IMGString)
                        ? null
                        : $"/resources/Profiles/{x.User.IMGString}",
                    AttachedImageUrl = x.Post.AttachedGalleryItemId.HasValue && x.Post.AttachedGalleryItem != null
                        ? $"/resources/Gallery/{x.Post.AttachedGalleryItemId}{x.Post.AttachedGalleryItem.FileExtension}"
                        : null,
                    LikesCount = x.Likes,
                    DislikesCount = x.Dislikes,
                    NetScore = x.NetScore,
                    CommentsCount = x.Post.Comments.Count(c => canSeeDeleted || !c.IsDeleted),
                    Tags = x.Tags,
                    TagMatchCount = x.TagMatchCount,
                    CanEdit = currentUserId.HasValue && x.Post.UserId == currentUserId.Value,
                    CanDelete = currentUserId.HasValue && (x.Post.UserId == currentUserId.Value || isAdmin),
                    CanRestore = currentUserId.HasValue && isAdmin && x.Post.IsDeleted,
                    CanModerate = currentUserId.HasValue && isAdmin
                })
            };
        }

        public async Task<object?> GetPostByIdAsync(
            int postId,
            int? currentUserId,
            bool includeDeleted,
            int commentCursor,
            int commentPageSize,
            int replyPageSize)
        {
            commentCursor = Math.Max(0, commentCursor);
            commentPageSize = Math.Clamp(commentPageSize, 1, 50);
            replyPageSize = Math.Clamp(replyPageSize, 1, 20);

            var isAdmin = currentUserId.HasValue && await IsAdminAsync(currentUserId.Value);
            var canSeeDeleted = includeDeleted && isAdmin;

            var postRow = await _context.ForumPost
                .Include(p => p.Reactions)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.ActivityTag)
                .Include(p => p.AttachedGalleryItem)
                .Join(
                    _context.User,
                    p => p.UserId,
                    u => u.Id,
                    (p, u) => new { Post = p, User = u })
                .GroupJoin(
                    _context.Role,
                    pu => pu.User.RoleId,
                    r => r.Id,
                    (pu, role) => new { pu.Post, pu.User, Role = role.FirstOrDefault() })
                .FirstOrDefaultAsync(x => x.Post.Id == postId);

            if (postRow == null) return null;
            if (postRow.Post.IsDeleted && !canSeeDeleted) return null;

            postRow.Post.ViewCount += 1;
            await _context.SaveChangesAsync();

            bool? myReaction = null;
            if (currentUserId.HasValue)
            {
                myReaction = await _context.ForumPostReaction
                    .Where(r => r.ForumPostId == postId && r.UserId == currentUserId.Value)
                    .Select(r => (bool?)r.IsLike)
                    .FirstOrDefaultAsync();
            }

            var topLevelQuery = _context.ForumComment
                .AsNoTracking()
                .Where(c => c.ForumPostId == postId && c.ParentCommentId == null)
                .Where(c => canSeeDeleted || !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAtUtc);

            var topLevelTotal = await topLevelQuery.CountAsync();
            var topLevelComments = await topLevelQuery
                .Skip(commentCursor)
                .Take(commentPageSize)
                .Join(
                    _context.User,
                    c => c.UserId,
                    u => u.Id,
                    (c, u) => new { Comment = c, User = u })
                .GroupJoin(
                    _context.Role,
                    cu => cu.User.RoleId,
                    r => r.Id,
                    (cu, role) => new { cu.Comment, cu.User, Role = role.FirstOrDefault() })
                .ToListAsync();

            var topLevelIds = topLevelComments.Select(x => x.Comment.Id).ToList();

            var repliesForTopLevel = await _context.ForumComment
                .AsNoTracking()
                .Where(c => c.ParentCommentId.HasValue && topLevelIds.Contains(c.ParentCommentId.Value))
                .Where(c => canSeeDeleted || !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAtUtc)
                .Join(
                    _context.User,
                    c => c.UserId,
                    u => u.Id,
                    (c, u) => new { Comment = c, User = u })
                .GroupJoin(
                    _context.Role,
                    cu => cu.User.RoleId,
                    r => r.Id,
                    (cu, role) => new { cu.Comment, cu.User, Role = role.FirstOrDefault() })
                .ToListAsync();

            var repliesByParent = repliesForTopLevel
                .GroupBy(x => x.Comment.ParentCommentId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var reactionMap = await _context.ForumCommentReaction
                .AsNoTracking()
                .Where(r => topLevelIds.Contains(r.ForumCommentId) || (r.ForumComment.ParentCommentId.HasValue && topLevelIds.Contains(r.ForumComment.ParentCommentId.Value)))
                .GroupBy(r => r.ForumCommentId)
                .Select(g => new
                {
                    CommentId = g.Key,
                    Likes = g.Count(x => x.IsLike),
                    Dislikes = g.Count(x => !x.IsLike)
                })
                .ToDictionaryAsync(x => x.CommentId, x => new { x.Likes, x.Dislikes });

            var myCommentReactions = currentUserId.HasValue
                ? await _context.ForumCommentReaction
                    .AsNoTracking()
                    .Where(r => r.UserId == currentUserId.Value)
                    .Where(r => topLevelIds.Contains(r.ForumCommentId) || (r.ForumComment.ParentCommentId.HasValue && topLevelIds.Contains(r.ForumComment.ParentCommentId.Value)))
                    .ToDictionaryAsync(r => r.ForumCommentId, r => (bool?)r.IsLike)
                : new Dictionary<int, bool?>();

            return new
            {
                postRow.Post.Id,
                postRow.Post.UserId,
                postRow.Post.Title,
                postRow.Post.Description,
                postRow.Post.CreatedAtUtc,
                postRow.Post.UpdatedAtUtc,
                postRow.Post.LastActivityAtUtc,
                postRow.Post.IsDeleted,
                postRow.Post.IsPinned,
                postRow.Post.IsLocked,
                postRow.Post.LockReason,
                postRow.Post.ViewCount,
                CanEdit = currentUserId.HasValue && postRow.Post.UserId == currentUserId.Value,
                CanDelete = currentUserId.HasValue && (postRow.Post.UserId == currentUserId.Value || isAdmin),
                CanRestore = currentUserId.HasValue && isAdmin && postRow.Post.IsDeleted,
                CanModerate = currentUserId.HasValue && isAdmin,
                AuthorName = BuildUserDisplayName(postRow.User, "Felhasználó"),
                AuthorRoleName = (postRow.Role?.Name ?? string.Empty).Trim(),
                ProfileImageUrl = string.IsNullOrWhiteSpace(postRow.User.IMGString)
                    ? null
                    : $"/resources/Profiles/{postRow.User.IMGString}",
                AttachedImageUrl = postRow.Post.AttachedGalleryItemId.HasValue && postRow.Post.AttachedGalleryItem != null
                    ? $"/resources/Gallery/{postRow.Post.AttachedGalleryItemId}{postRow.Post.AttachedGalleryItem.FileExtension}"
                    : null,
                LikesCount = postRow.Post.Reactions.Count(r => r.IsLike),
                DislikesCount = postRow.Post.Reactions.Count(r => !r.IsLike),
                MyReaction = myReaction,
                Tags = postRow.Post.PostTags
                    .Select(pt => (pt.ActivityTag.Activity ?? string.Empty).Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                CommentsCursor = commentCursor,
                NextCommentCursor = commentCursor + topLevelComments.Count,
                HasMoreComments = topLevelTotal > commentCursor + topLevelComments.Count,
                Comments = topLevelComments.Select(c => SerializeComment(
                    c.Comment,
                    c.User,
                    c.Role?.Name,
                    postRow.Post,
                    currentUserId,
                    isAdmin,
                    reactionMap.ToDictionary(kvp => kvp.Key, kvp => (dynamic)kvp.Value),
                    myCommentReactions,
                    repliesByParent.TryGetValue(c.Comment.Id, out var children)
                        ? children.Cast<dynamic>().ToList()
                        : new List<dynamic>(),
                    replyPageSize,
                    canSeeDeleted))
            };
        }

        public async Task<object> GetCommentRepliesAsync(
            int commentId,
            int cursor,
            int pageSize,
            int? currentUserId,
            bool includeDeleted)
        {
            cursor = Math.Max(0, cursor);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var parent = await _context.ForumComment
                .AsNoTracking()
                .Include(c => c.ForumPost)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (parent == null)
            {
                return new { Found = false };
            }

            var isAdmin = currentUserId.HasValue && await IsAdminAsync(currentUserId.Value);
            var canSeeDeleted = includeDeleted && isAdmin;

            if (parent.ForumPost.IsDeleted && !canSeeDeleted)
            {
                return new { Found = false };
            }

            var repliesQuery = _context.ForumComment
                .AsNoTracking()
                .Where(c => c.ParentCommentId == commentId)
                .Where(c => canSeeDeleted || !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAtUtc);

            var total = await repliesQuery.CountAsync();
            var rows = await repliesQuery
                .Skip(cursor)
                .Take(pageSize)
                .Join(_context.User, c => c.UserId, u => u.Id, (c, u) => new { Comment = c, User = u })
                .GroupJoin(_context.Role, cu => cu.User.RoleId, r => r.Id, (cu, role) => new { cu.Comment, cu.User, Role = role.FirstOrDefault() })
                .ToListAsync();

            var ids = rows.Select(x => x.Comment.Id).ToList();

            var reactionMap = await _context.ForumCommentReaction
                .AsNoTracking()
                .Where(r => ids.Contains(r.ForumCommentId))
                .GroupBy(r => r.ForumCommentId)
                .Select(g => new
                {
                    CommentId = g.Key,
                    Likes = g.Count(x => x.IsLike),
                    Dislikes = g.Count(x => !x.IsLike)
                })
                .ToDictionaryAsync(x => x.CommentId, x => new { x.Likes, x.Dislikes });

            var myCommentReactions = currentUserId.HasValue
                ? await _context.ForumCommentReaction
                    .AsNoTracking()
                    .Where(r => r.UserId == currentUserId.Value && ids.Contains(r.ForumCommentId))
                    .ToDictionaryAsync(r => r.ForumCommentId, r => (bool?)r.IsLike)
                : new Dictionary<int, bool?>();

            return new
            {
                Found = true,
                Cursor = cursor,
                NextCursor = cursor + rows.Count,
                HasMore = total > cursor + rows.Count,
                Replies = rows.Select(x => new
                {
                    x.Comment.Id,
                    x.Comment.ParentCommentId,
                    x.Comment.UserId,
                    Message = x.Comment.IsDeleted ? "[Törölt hozzászólás]" : x.Comment.Message,
                    x.Comment.IsDeleted,
                    x.Comment.CreatedAtUtc,
                    x.Comment.UpdatedAtUtc,
                    AuthorName = BuildUserDisplayName(x.User, "Felhasználó"),
                    AuthorRoleName = (x.Role?.Name ?? string.Empty).Trim(),
                    ProfileImageUrl = string.IsNullOrWhiteSpace(x.User.IMGString)
                        ? null
                        : $"/resources/Profiles/{x.User.IMGString}",
                    LikesCount = reactionMap.TryGetValue(x.Comment.Id, out var reaction) ? reaction.Likes : 0,
                    DislikesCount = reactionMap.TryGetValue(x.Comment.Id, out reaction) ? reaction.Dislikes : 0,
                    MyReaction = myCommentReactions.TryGetValue(x.Comment.Id, out var myReaction) ? myReaction : null,
                    Depth = GetDepthFromPath(rows.ToDictionary(r => r.Comment.Id, r => r.Comment.ParentCommentId), x.Comment),
                    CanDelete = currentUserId.HasValue && (x.Comment.UserId == currentUserId.Value || parent.ForumPost.UserId == currentUserId.Value || isAdmin),
                    CanRestore = currentUserId.HasValue && isAdmin && x.Comment.IsDeleted,
                    HasMoreReplies = false,
                    NextReplyCursor = 0,
                    Replies = new List<object>()
                })
            };
        }

        public async Task<object> CreatePostAsync(int userId, string title, string description, int? attachedGalleryItemId, List<string>? tagNames)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("A cím kötelező.");
            title = title.Trim();
            description = (description ?? string.Empty).Trim();
            if (title.Length > 150) throw new ArgumentException("A cím túl hosszú.");
            if (description.Length > 2000) throw new ArgumentException("A leírás túl hosszú.");

            if (attachedGalleryItemId.HasValue)
            {
                var attachmentValid = await IsAttachmentValidAsync(userId, attachedGalleryItemId.Value);
                if (!attachmentValid) throw new ArgumentException("Csak saját, nem törölt galéria képet csatolhatsz.");
            }

            var post = new ForumPost
            {
                UserId = userId,
                Title = title,
                Description = description,
                AttachedGalleryItemId = attachedGalleryItemId,
                CreatedAtUtc = DateTime.UtcNow,
                LastActivityAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _context.ForumPost.Add(post);
            await _context.SaveChangesAsync();

            await UpsertPostTagsAsync(post.Id, userId, tagNames);

            return new { post.Id };
        }

        public async Task<bool> UpdatePostAsync(int postId, int userId, string title, string description, int? attachedGalleryItemId, List<string>? tagNames)
        {
            var post = await _context.ForumPost.FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted);
            if (post == null) return false;

            var isAdmin = await IsAdminAsync(userId);
            if (!isAdmin && post.UserId != userId) return false;

            if (attachedGalleryItemId.HasValue)
            {
                var attachmentValid = await IsAttachmentValidAsync(post.UserId, attachedGalleryItemId.Value);
                if (!attachmentValid) return false;
            }

            title = (title ?? string.Empty).Trim();
            description = (description ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(title) || title.Length > 150 || description.Length > 2000) return false;

            post.Title = title;
            post.Description = description;
            post.AttachedGalleryItemId = attachedGalleryItemId;
            post.UpdatedAtUtc = DateTime.UtcNow;

            await UpsertPostTagsAsync(post.Id, userId, tagNames);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePostAsync(int postId, int userId)
        {
            var post = await _context.ForumPost.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null) return false;

            var isAdmin = await IsAdminAsync(userId);
            if (!isAdmin && post.UserId != userId) return false;
            if (post.IsDeleted) return true;

            post.IsDeleted = true;
            post.DeletedAtUtc = DateTime.UtcNow;
            post.DeletedByUserId = userId;
            post.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestorePostAsync(int postId, int userId)
        {
            var isAdmin = await IsAdminAsync(userId);
            if (!isAdmin) return false;

            var post = await _context.ForumPost.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null || !post.IsDeleted) return false;

            post.IsDeleted = false;
            post.DeletedAtUtc = null;
            post.DeletedByUserId = null;
            post.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetPinnedStateAsync(int postId, int userId, bool isPinned)
        {
            var isAdmin = await IsAdminAsync(userId);
            if (!isAdmin) return false;

            var post = await _context.ForumPost.FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted);
            if (post == null) return false;

            post.IsPinned = isPinned;
            post.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetLockedStateAsync(int postId, int userId, bool isLocked, string? reason)
        {
            var isAdmin = await IsAdminAsync(userId);
            if (!isAdmin) return false;

            var post = await _context.ForumPost.FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted);
            if (post == null) return false;

            post.IsLocked = isLocked;
            post.LockReason = isLocked ? (reason ?? string.Empty).Trim() : null;
            post.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<object> AddCommentAsync(int postId, int userId, string message, int? parentCommentId)
        {
            var post = await _context.ForumPost.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null || post.IsDeleted) throw new ArgumentException("A bejegyzés nem található.");
            if (post.IsLocked) throw new UnauthorizedAccessException("A bejegyzés le van zárva.");

            message = (message ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Az üzenet nem lehet üres.");
            if (message.Length > 1000) throw new ArgumentException("Az üzenet túl hosszú.");

            if (parentCommentId.HasValue)
            {
                var parent = await _context.ForumComment.FirstOrDefaultAsync(c => c.Id == parentCommentId.Value && c.ForumPostId == postId);
                if (parent == null) throw new ArgumentException("A szülő hozzászólás nem található.");

                var parentDepth = await GetCommentDepthAsync(parent.Id);
                if (parentDepth >= MaxDepth) throw new ArgumentException("Maximum 3 szintű válasz engedélyezett.");
            }

            var comment = new ForumComment
            {
                ForumPostId = postId,
                ParentCommentId = parentCommentId,
                UserId = userId,
                Message = message,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _context.ForumComment.Add(comment);
            post.LastActivityAtUtc = DateTime.UtcNow;
            post.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new { comment.Id };
        }

        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _context.ForumComment
                .Include(c => c.ForumPost)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null) return false;
            if (comment.IsDeleted) return true;

            var isAdmin = await IsAdminAsync(userId);
            var canDelete = comment.UserId == userId || comment.ForumPost.UserId == userId || isAdmin;
            if (!canDelete) return false;

            comment.IsDeleted = true;
            comment.DeletedAtUtc = DateTime.UtcNow;
            comment.DeletedByUserId = userId;
            comment.UpdatedAtUtc = DateTime.UtcNow;
            comment.ForumPost.LastActivityAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreCommentAsync(int commentId, int userId)
        {
            var isAdmin = await IsAdminAsync(userId);
            if (!isAdmin) return false;

            var comment = await _context.ForumComment
                .Include(c => c.ForumPost)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null || !comment.IsDeleted) return false;

            comment.IsDeleted = false;
            comment.DeletedAtUtc = null;
            comment.DeletedByUserId = null;
            comment.UpdatedAtUtc = DateTime.UtcNow;
            comment.ForumPost.LastActivityAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TogglePostReactionAsync(int postId, int userId, bool isLike)
        {
            var post = await _context.ForumPost.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null || post.IsDeleted) return false;
            if (post.IsLocked) return false;

            var reaction = await _context.ForumPostReaction.FirstOrDefaultAsync(r => r.ForumPostId == postId && r.UserId == userId);
            if (reaction == null)
            {
                _context.ForumPostReaction.Add(new ForumPostReaction
                {
                    ForumPostId = postId,
                    UserId = userId,
                    IsLike = isLike,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
            else if (reaction.IsLike == isLike)
            {
                _context.ForumPostReaction.Remove(reaction);
            }
            else
            {
                reaction.IsLike = isLike;
                reaction.CreatedAtUtc = DateTime.UtcNow;
            }

            post.LastActivityAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleCommentReactionAsync(int commentId, int userId, bool isLike)
        {
            var comment = await _context.ForumComment
                .Include(c => c.ForumPost)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null || comment.IsDeleted) return false;
            if (comment.ForumPost.IsLocked) return false;

            var reaction = await _context.ForumCommentReaction.FirstOrDefaultAsync(r => r.ForumCommentId == commentId && r.UserId == userId);
            if (reaction == null)
            {
                _context.ForumCommentReaction.Add(new ForumCommentReaction
                {
                    ForumCommentId = commentId,
                    UserId = userId,
                    IsLike = isLike,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
            else if (reaction.IsLike == isLike)
            {
                _context.ForumCommentReaction.Remove(reaction);
            }
            else
            {
                reaction.IsLike = isLike;
                reaction.CreatedAtUtc = DateTime.UtcNow;
            }

            comment.ForumPost.LastActivityAtUtc = DateTime.UtcNow;
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

        private static List<string> NormalizeTags(List<string>? tags)
        {
            return (tags ?? new List<string>())
                .Select(t => (t ?? string.Empty).Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task<bool> IsAttachmentValidAsync(int ownerUserId, int galleryItemId)
        {
            return await _context.GalleryItem
                .AnyAsync(g => g.Id == galleryItemId && g.UserId == ownerUserId && !g.IsDeleted);
        }

        private async Task UpsertPostTagsAsync(int postId, int userId, List<string>? tagNames)
        {
            var normalizedTags = NormalizeTags(tagNames);

            var existing = await _context.ForumPostTag.Where(x => x.ForumPostId == postId).ToListAsync();
            _context.ForumPostTag.RemoveRange(existing);

            if (normalizedTags.Count == 0) return;

            var dbTags = await _context.ActivityTag
                .Where(t => t.Activity != null)
                .ToListAsync();

            var tagLookup = dbTags
                .GroupBy(t => t.Activity!.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var canCreate = await CanCreateForumTagAsync(userId);
            foreach (var tagName in normalizedTags)
            {
                if (!tagLookup.TryGetValue(tagName, out var tag))
                {
                    if (!canCreate) continue;
                    tag = new Libary.Model.Tag.ActivityTag { Activity = tagName };
                    _context.ActivityTag.Add(tag);
                    await _context.SaveChangesAsync();
                    tagLookup[tagName] = tag;
                }

                _context.ForumPostTag.Add(new ForumPostTag
                {
                    ForumPostId = postId,
                    TagId = tag.Id
                });
            }
        }

        private async Task<bool> CanCreateForumTagAsync(int userId)
        {
            var roleData = await _context.User
                .Where(u => u.Id == userId)
                .Join(
                    _context.Role,
                    u => u.RoleId,
                    r => r.Id,
                    (u, r) => new { u.RoleId, RoleName = (r.Name ?? string.Empty).Trim().ToLower() })
                .FirstOrDefaultAsync();

            if (roleData == null) return false;
            if (roleData.RoleId == 1) return true;
            if (roleData.RoleId == 3 || roleData.RoleId == 4 || roleData.RoleId == 5) return true;

            return roleData.RoleName.Contains("kert");
        }

        private async Task<int> GetCommentDepthAsync(int commentId)
        {
            var depth = 1;
            var current = await _context.ForumComment.AsNoTracking().FirstOrDefaultAsync(c => c.Id == commentId);
            while (current?.ParentCommentId != null)
            {
                depth += 1;
                current = await _context.ForumComment.AsNoTracking().FirstOrDefaultAsync(c => c.Id == current.ParentCommentId.Value);
                if (depth > MaxDepth + 2) break;
            }
            return depth;
        }

        private static object SerializeComment(
            ForumComment comment,
            Libary.Model.User.User user,
            string? roleName,
            ForumPost post,
            int? currentUserId,
            bool isAdmin,
            IDictionary<int, dynamic> reactionMap,
            IDictionary<int, bool?> myCommentReactions,
            List<dynamic> replyRows,
            int replyPageSize,
            bool canSeeDeleted)
        {
            var orderedReplies = replyRows
                .OrderByDescending(r => ((ForumComment)r.Comment).CreatedAtUtc)
                .ToList();

            var initialReplies = orderedReplies.Take(replyPageSize).ToList();

            return new
            {
                comment.Id,
                comment.ParentCommentId,
                comment.UserId,
                Message = comment.IsDeleted ? "[Törölt hozzászólás]" : comment.Message,
                comment.IsDeleted,
                comment.CreatedAtUtc,
                comment.UpdatedAtUtc,
                AuthorName = BuildUserDisplayName(user, "Felhasználó"),
                AuthorRoleName = (roleName ?? string.Empty).Trim(),
                ProfileImageUrl = string.IsNullOrWhiteSpace(user.IMGString)
                    ? null
                    : $"/resources/Profiles/{user.IMGString}",
                LikesCount = reactionMap.TryGetValue(comment.Id, out var reaction) ? reaction.Likes : 0,
                DislikesCount = reactionMap.TryGetValue(comment.Id, out reaction) ? reaction.Dislikes : 0,
                MyReaction = myCommentReactions.TryGetValue(comment.Id, out var myReaction) ? myReaction : null,
                Depth = 1,
                CanDelete = currentUserId.HasValue && (comment.UserId == currentUserId.Value || post.UserId == currentUserId.Value || isAdmin),
                CanRestore = currentUserId.HasValue && isAdmin && comment.IsDeleted,
                HasMoreReplies = orderedReplies.Count > initialReplies.Count,
                NextReplyCursor = initialReplies.Count,
                Replies = initialReplies.Select(reply =>
                {
                    var replyComment = (ForumComment)reply.Comment;
                    var replyUser = (Libary.Model.User.User)reply.User;
                    var replyRole = reply.Role?.Name as string;
                    return new
                    {
                        replyComment.Id,
                        replyComment.ParentCommentId,
                        replyComment.UserId,
                        Message = replyComment.IsDeleted ? "[Törölt hozzászólás]" : replyComment.Message,
                        replyComment.IsDeleted,
                        replyComment.CreatedAtUtc,
                        replyComment.UpdatedAtUtc,
                        AuthorName = BuildUserDisplayName(replyUser, "Felhasználó"),
                        AuthorRoleName = (replyRole ?? string.Empty).Trim(),
                        ProfileImageUrl = string.IsNullOrWhiteSpace(replyUser.IMGString)
                            ? null
                            : $"/resources/Profiles/{replyUser.IMGString}",
                        LikesCount = reactionMap.TryGetValue(replyComment.Id, out var replyReaction) ? replyReaction.Likes : 0,
                        DislikesCount = reactionMap.TryGetValue(replyComment.Id, out replyReaction) ? replyReaction.Dislikes : 0,
                        MyReaction = myCommentReactions.TryGetValue(replyComment.Id, out var replyMyReaction) ? replyMyReaction : null,
                        Depth = 2,
                        CanDelete = currentUserId.HasValue && (replyComment.UserId == currentUserId.Value || post.UserId == currentUserId.Value || isAdmin),
                        CanRestore = currentUserId.HasValue && isAdmin && replyComment.IsDeleted,
                        HasMoreReplies = false,
                        NextReplyCursor = 0,
                        Replies = new List<object>()
                    };
                })
            };
        }

        private static int GetDepthFromPath(IDictionary<int, int?> parentMap, ForumComment comment)
        {
            var depth = 1;
            var parentId = comment.ParentCommentId;
            while (parentId.HasValue)
            {
                depth += 1;
                if (!parentMap.TryGetValue(parentId.Value, out parentId)) break;
            }
            return depth;
        }

        private static string BuildUserDisplayName(Libary.Model.User.User user, string fallbackUsername)
        {
            if (string.IsNullOrWhiteSpace(user.VezetekNev) || string.IsNullOrWhiteSpace(user.KeresztNev))
            {
                return fallbackUsername;
            }

            return $"{user.VezetekNev} {user.KeresztNev}";
        }
    }
}
