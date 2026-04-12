using Kerting_Api.DTO;
using Kerting_Api.Interface;
using Libary;
using Libary.Model.User;
using Microsoft.EntityFrameworkCore;

namespace Kerting_Api.Service
{
    /// <summary>
    /// Felhasználói értékelések üzleti logikája.
    /// Kezeli a hierarchikus kommentfolyamot, reakciókat, törlést és visszaállítást.
    /// </summary>
    public sealed class UserReviewService : IUserReviewService
    {
        private readonly KertingDbContext _context;

        public UserReviewService(KertingDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Értékelés-folyam összeállítása:
        /// - top-level értékelések,
        /// - valaszok,
        /// - like/dislike statisztikák,
        /// - aktuális user reakciója.
        /// </summary>
        public async Task<object> GetReviewsAsync(int targetUserId, int? currentUserId)
        {
            var isAdmin = currentUserId.HasValue && await IsAdminAsync(currentUserId.Value);

            var topLevelReviews = await _context.UserReview
                .AsNoTracking()
                .Where(r => r.TargetUserId == targetUserId && r.ParentReviewId == null)
                .Where(r => !r.IsDeleted || isAdmin)
                .OrderByDescending(r => r.CreatedAtUtc)
                .Join(
                    _context.User,
                    r => r.AuthorUserId,
                    u => u.Id,
                    (r, u) => new { Review = r, Author = u }
                )
                .ToListAsync();

            var topLevelIds = topLevelReviews.Select(x => x.Review.Id).ToList();

            var replies = await _context.UserReview
                .AsNoTracking()
                .Where(r => r.ParentReviewId.HasValue && topLevelIds.Contains(r.ParentReviewId.Value))
                .Where(r => !r.IsDeleted || isAdmin)
                .OrderBy(r => r.CreatedAtUtc)
                .Join(
                    _context.User,
                    r => r.AuthorUserId,
                    u => u.Id,
                    (r, u) => new { Review = r, Author = u }
                )
                .ToListAsync();

            var repliesByParent = replies
                .GroupBy(x => x.Review.ParentReviewId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var allReviewIds = topLevelIds.Concat(replies.Select(x => x.Review.Id)).ToList();

            var reactionMap = await _context.UserReviewReaction
                .AsNoTracking()
                .Where(r => allReviewIds.Contains(r.UserReviewId))
                .GroupBy(r => r.UserReviewId)
                .Select(g => new
                {
                    ReviewId = g.Key,
                    LikesCount = g.Count(x => x.IsLike),
                    DislikesCount = g.Count(x => !x.IsLike)
                })
                .ToDictionaryAsync(x => x.ReviewId, x => new { x.LikesCount, x.DislikesCount });

            var myReactions = currentUserId.HasValue
                ? await _context.UserReviewReaction
                    .AsNoTracking()
                    .Where(r => r.UserId == currentUserId.Value && allReviewIds.Contains(r.UserReviewId))
                    .ToDictionaryAsync(r => r.UserReviewId, r => (bool?)r.IsLike)
                : new Dictionary<int, bool?>();

            // A frontend által elvárt lapos + nested válaszstruktúra.
            return topLevelReviews.Select(tr => new
            {
                id = tr.Review.Id,
                authorName = BuildDisplayName(tr.Author),
                rating = tr.Review.Rating,
                message = (tr.Review.IsDeleted && !isAdmin) ? "[Törölt értékelés]" : tr.Review.Message,
                isDeleted = tr.Review.IsDeleted,
                createdAtUtc = tr.Review.CreatedAtUtc,
                likesCount = reactionMap.TryGetValue(tr.Review.Id, out var rStat) ? rStat.LikesCount : 0,
                dislikesCount = reactionMap.TryGetValue(tr.Review.Id, out rStat) ? rStat.DislikesCount : 0,
                myReaction = myReactions.TryGetValue(tr.Review.Id, out var myReact) ? myReact : null,
                canDelete = currentUserId == tr.Review.AuthorUserId || isAdmin,
                canRestore = isAdmin && tr.Review.IsDeleted,
                replies = repliesByParent.TryGetValue(tr.Review.Id, out var repList)
                    ? repList.Select(rep => (object)new
                    {
                        id = rep.Review.Id,
                        authorName = BuildDisplayName(rep.Author),
                        message = (rep.Review.IsDeleted && !isAdmin) ? "[Törölt válasz]" : rep.Review.Message,
                        isDeleted = rep.Review.IsDeleted,
                        createdAtUtc = rep.Review.CreatedAtUtc,
                        likesCount = reactionMap.TryGetValue(rep.Review.Id, out var repStat) ? repStat.LikesCount : 0,
                        dislikesCount = reactionMap.TryGetValue(rep.Review.Id, out repStat) ? repStat.DislikesCount : 0,
                        myReaction = myReactions.TryGetValue(rep.Review.Id, out var repReact) ? repReact : null,
                        canDelete = currentUserId == rep.Review.AuthorUserId || isAdmin,
                        canRestore = isAdmin && rep.Review.IsDeleted,
                    }).ToList()
                    : new List<object>()
            });
        }

        /// <summary>
        /// Új értékelés vagy válasz létrehozása szabályellenőrzésekkel.
        /// </summary>
        public async Task AddReviewAsync(int targetUserId, int userId, AddReviewRequest request)
        {
            if (userId == targetUserId)
            {
                throw new InvalidOperationException("Saját magadat nem értékelheted.");
            }

            var targetUserExists = await _context.User.AnyAsync(u => u.Id == targetUserId);
            if (!targetUserExists)
            {
                throw new KeyNotFoundException("Az értékelt felhasználó nem található.");
            }

            if (request.ParentReviewId == null && (request.Rating == null || request.Rating < 1 || request.Rating > 5))
            {
                throw new InvalidOperationException("A fő értékeléshez kötelező pontszámot (1-5) megadni.");
            }

            var review = new UserReview
            {
                TargetUserId = targetUserId,
                AuthorUserId = userId,
                ParentReviewId = request.ParentReviewId,
                Rating = request.ParentReviewId == null ? request.Rating : null,
                Message = request.Message.Trim(),
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _context.UserReview.Add(review);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Like/dislike reakció kapcsolása:
        /// - ha nincs reakció: új létrehozás,
        /// - ha ugyanazra nyom: törlés,
        /// - ha másikra vált: frissítés.
        /// </summary>
        public async Task ToggleReactionAsync(int reviewId, int userId, bool isLike)
        {
            var review = await _context.UserReview.FirstOrDefaultAsync(r => r.Id == reviewId);
            if (review == null || review.IsDeleted)
            {
                throw new KeyNotFoundException();
            }

            var reaction = await _context.UserReviewReaction
                .FirstOrDefaultAsync(r => r.UserReviewId == reviewId && r.UserId == userId);

            if (reaction == null)
            {
                _context.UserReviewReaction.Add(new UserReviewReaction
                {
                    UserReviewId = reviewId,
                    UserId = userId,
                    IsLike = isLike,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
            else if (reaction.IsLike == isLike)
            {
                _context.UserReviewReaction.Remove(reaction);
            }
            else
            {
                reaction.IsLike = isLike;
                reaction.CreatedAtUtc = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Értékelés törlése:
        /// - első törlés: soft delete,
        /// - már törölt elemen újabb törlés: hard delete.
        /// </summary>
        public async Task DeleteReviewAsync(int reviewId, int userId)
        {
            var isAdmin = await IsAdminAsync(userId);

            var review = await _context.UserReview.FirstOrDefaultAsync(r => r.Id == reviewId);
            if (review == null)
            {
                throw new KeyNotFoundException();
            }

            if (review.AuthorUserId != userId && !isAdmin)
            {
                throw new UnauthorizedAccessException();
            }

            if (review.IsDeleted)
            {
                var replies = await _context.UserReview.Where(r => r.ParentReviewId == reviewId).ToListAsync();

                if (replies.Any())
                {
                    _context.UserReview.RemoveRange(replies);
                    await _context.SaveChangesAsync();
                }

                _context.UserReview.Remove(review);
            }
            else
            {
                review.IsDeleted = true;
                review.DeletedAtUtc = DateTime.UtcNow;
                review.DeletedByUserId = userId;
                review.UpdatedAtUtc = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Törölt értékelés visszaállítása admin által.
        /// </summary>
        public async Task RestoreReviewAsync(int reviewId, int userId)
        {
            if (!await IsAdminAsync(userId))
            {
                throw new UnauthorizedAccessException();
            }

            var review = await _context.UserReview.FirstOrDefaultAsync(r => r.Id == reviewId);
            if (review == null)
            {
                throw new KeyNotFoundException();
            }

            if (review.IsDeleted)
            {
                review.IsDeleted = false;
                review.DeletedAtUtc = null;
                review.DeletedByUserId = null;
                review.UpdatedAtUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // Admin jogosultság ellenőrzés user ID alapján.
        private async Task<bool> IsAdminAsync(int userId)
        {
            return await _context.User
                .Where(u => u.Id == userId)
                .Select(u => u.RoleId == 1)
                .FirstOrDefaultAsync();
        }

            // Egységes név-formázás az értékelés listában.
        private static string BuildDisplayName(User user)
        {
            return $"{user.VezetekNev} {user.KeresztNev}".Trim();
        }
    }
}