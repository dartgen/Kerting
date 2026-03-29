using Libary;
using Libary.Model.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Kerting_Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserReviewController : ControllerBase
    {
        private readonly KertingDbContext _context;

        public UserReviewController(KertingDbContext context)
        {
            _context = context;
        }

        private int? TryGetCurrentUserId()
        {
            var rawId = User.FindFirst("Id")?.Value;
            return int.TryParse(rawId, out var userId) ? userId : null;
        }
        private async Task<bool> IsAdminAsync(int userId)
        {
            return await _context.User
                .Where(u => u.Id == userId)
                .Select(u => u.RoleId == 1) // Itt ellenőrizzük az Admin RoleId-t
                .FirstOrDefaultAsync();
        }
        public sealed class AddReviewRequest
        {
            public int? ParentReviewId { get; set; }
            public byte? Rating { get; set; }

            [Required]
            [MaxLength(2000)]
            public string Message { get; set; } = string.Empty;
        }

        // 1. ÉRTÉKELÉSEK LEKÉRDEZÉSE (PUBLIKUS)
        [HttpGet("{targetUserId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviews(int targetUserId)
        {
            var currentUserId = TryGetCurrentUserId();
            var isAdmin = currentUserId.HasValue && await IsAdminAsync(currentUserId.Value);

            // Csak a fő értékeléseket kérjük le első körben
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

            // Lekérjük a válaszokat
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

            var repliesByParent = replies.GroupBy(x => x.Review.ParentReviewId!.Value).ToDictionary(g => g.Key, g => g.ToList());

            // Lekérjük a reakciókat (Like / Dislike számolás)
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

            // Aktuális felhasználó reakciói
            var myReactions = currentUserId.HasValue
                ? await _context.UserReviewReaction
                    .AsNoTracking()
                    .Where(r => r.UserId == currentUserId.Value && allReviewIds.Contains(r.UserReviewId))
                    .ToDictionaryAsync(r => r.UserReviewId, r => (bool?)r.IsLike)
                : new Dictionary<int, bool?>();

            // Összeállítjuk a Vue által várt JSON választ
            var result = topLevelReviews.Select(tr => new
            {
                id = tr.Review.Id,
                authorName = $"{tr.Author.VezetekNev} {tr.Author.KeresztNev}".Trim(),
                rating = tr.Review.Rating,

                // ÚJ RÉSZ: Ha törölt, de ADMIN nézi, akkor mutassa a szöveget. Ha normál user, marad a [Törölt...]
                message = (tr.Review.IsDeleted && !isAdmin) ? "[Törölt értékelés]" : tr.Review.Message,
                isDeleted = tr.Review.IsDeleted, // <-- EZ AZ ÚJ MEZŐ

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
                        authorName = $"{rep.Author.VezetekNev} {rep.Author.KeresztNev}".Trim(),

                        // ÚJ RÉSZ A VÁLASZOKNÁL IS:
                        message = (rep.Review.IsDeleted && !isAdmin) ? "[Törölt válasz]" : rep.Review.Message,
                        isDeleted = rep.Review.IsDeleted, // <-- EZ AZ ÚJ MEZŐ

                        createdAtUtc = rep.Review.CreatedAtUtc,
                        likesCount = reactionMap.TryGetValue(rep.Review.Id, out var repStat) ? repStat.LikesCount : 0,
                        dislikesCount = reactionMap.TryGetValue(rep.Review.Id, out repStat) ? repStat.DislikesCount : 0,
                        myReaction = myReactions.TryGetValue(rep.Review.Id, out var repReact) ? repReact : null,
                        canDelete = currentUserId == rep.Review.AuthorUserId || isAdmin,
                        canRestore = isAdmin && rep.Review.IsDeleted,
                    }).ToList()
                    : new List<object>()
            });

            return Ok(result);
        }

        // 2. ÉRTÉKELÉS VAGY VÁLASZ BEKÜLDÉSE
        [Authorize]
        [HttpPost("{targetUserId}")]
        public async Task<IActionResult> AddReview(int targetUserId, [FromBody] AddReviewRequest request)
        {
            var userId = TryGetCurrentUserId()!.Value;

            if (userId == targetUserId) return BadRequest("Saját magadat nem értékelheted.");

            var targetUserExists = await _context.User.AnyAsync(u => u.Id == targetUserId);
            if (!targetUserExists) return NotFound("Az értékelt felhasználó nem található.");

            // Ha ez egy fő értékelés (nem válasz), akkor kötelező a csillag
            if (request.ParentReviewId == null && (request.Rating == null || request.Rating < 1 || request.Rating > 5))
            {
                return BadRequest("A fő értékeléshez kötelező pontszámot (1-5) megadni.");
            }

            var review = new UserReview
            {
                TargetUserId = targetUserId,
                AuthorUserId = userId,
                ParentReviewId = request.ParentReviewId,
                Rating = request.ParentReviewId == null ? request.Rating : null, // Válasznál a rating NULL
                Message = request.Message.Trim(),
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _context.UserReview.Add(review);
            await _context.SaveChangesAsync();

            return Ok(new { review.Id });
        }

        // 3. REAKCIÓ (LIKE / DISLIKE)
        [Authorize]
        [HttpPost("{reviewId}/react")]
        public async Task<IActionResult> ToggleReaction(int reviewId, [FromQuery] bool isLike)
        {
            var userId = TryGetCurrentUserId()!.Value;

            var review = await _context.UserReview.FirstOrDefaultAsync(r => r.Id == reviewId);
            if (review == null || review.IsDeleted) return NotFound();

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
                _context.UserReviewReaction.Remove(reaction); // Ha ugyanarra nyom, leveszi
            }
            else
            {
                reaction.IsLike = isLike; // Ha megváltoztatja (pl. Like-ról Dislike-ra)
                reaction.CreatedAtUtc = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // 4. ÉRTÉKELÉS TÖRLÉSE (Soft Delete, majd Hard Delete)
        [Authorize]
        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var userId = TryGetCurrentUserId()!.Value;
            var isAdmin = await IsAdminAsync(userId);

            var review = await _context.UserReview.FirstOrDefaultAsync(r => r.Id == reviewId);
            if (review == null) return NotFound();

            if (review.AuthorUserId != userId && !isAdmin) return Forbid();

            if (review.IsDeleted)
            {
                // --- VÉGLEGES TÖRLÉS (Hard Delete) ---

                // 1. Megkeressük az összes választ, ami erre az értékelésre érkezett
                var replies = await _context.UserReview.Where(r => r.ParentReviewId == reviewId).ToListAsync();

                if (replies.Any())
                {
                    // 2. Töröljük a válaszokat
                    _context.UserReview.RemoveRange(replies);

                    // 3. FONTOS: Először a válaszok törlését el kell mentenünk az adatbázisba, 
                    // hogy megszűnjön a kényszer (Constraint) konfliktus!
                    await _context.SaveChangesAsync();
                }

                // 4. Most, hogy már nincsenek válaszok, törölhető maga a fő értékelés is
                _context.UserReview.Remove(review);
            }
            else
            {
                // --- LOGIKAI TÖRLÉS (Soft Delete) ---
                review.IsDeleted = true;
                review.DeletedAtUtc = DateTime.UtcNow;
                review.DeletedByUserId = userId;
                review.UpdatedAtUtc = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
        // 5. VISSZAÁLLÍTÁS (Restore)
        [Authorize]
        [HttpPatch("{reviewId}/restore")]
        public async Task<IActionResult> RestoreReview(int reviewId)
        {
            var userId = TryGetCurrentUserId()!.Value;
            var isAdmin = await IsAdminAsync(userId);

            // Csak admin állíthat vissza
            if (!isAdmin) return Forbid();

            var review = await _context.UserReview.FirstOrDefaultAsync(r => r.Id == reviewId);
            if (review == null) return NotFound();

            if (review.IsDeleted)
            {
                review.IsDeleted = false;
                review.DeletedAtUtc = null;
                review.DeletedByUserId = null;
                review.UpdatedAtUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
