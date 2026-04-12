using Kerting_Api.DTO;

namespace Kerting_Api.Interface
{
    /// <summary>
    /// Felhasználói értékelések és reakciók szerződése.
    /// </summary>
    public interface IUserReviewService
    {
        /// <summary>
        /// Értékelés folyam lekérése top-level + válaszok szerkezettel.
        /// </summary>
        Task<object> GetReviewsAsync(int targetUserId, int? currentUserId);

        /// <summary>
        /// Új értékelés vagy válasz létrehozása.
        /// </summary>
        Task AddReviewAsync(int targetUserId, int userId, AddReviewRequest request);

        /// <summary>
        /// Like/dislike reakció kapcsolása.
        /// </summary>
        Task ToggleReactionAsync(int reviewId, int userId, bool isLike);

        /// <summary>
        /// Értékelés törlése (soft/hard logika szerint).
        /// </summary>
        Task DeleteReviewAsync(int reviewId, int userId);

        /// <summary>
        /// Értékelés visszaállítása admin által.
        /// </summary>
        Task RestoreReviewAsync(int reviewId, int userId);
    }
}