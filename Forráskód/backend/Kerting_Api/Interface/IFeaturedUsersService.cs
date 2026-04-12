using Kerting_Api.DTO;

namespace Kerting_Api.Interface
{
    /// <summary>
    /// Kiemelt felhasználó modul szerződése.
    /// </summary>
    public interface IFeaturedUsersService
    {
        /// <summary>
        /// Főoldali carousel elemek listázása slot sorrendben.
        /// </summary>
        Task<List<FeaturedCarouselItemDto>> GetFeaturedUsersForCarouselAsync();

        /// <summary>
        /// Admin oldalhoz szükséges slot + választható user adatok.
        /// </summary>
        Task<AdminFeaturedDataDto> GetAdminFeaturedDataAsync(int userId);

        /// <summary>
        /// Slot kiosztás mentése admin jogosultsággal.
        /// </summary>
        Task UpsertFeaturedSlotsAsync(int userId, List<FeaturedSlotUpsertDto> request);
    }
}