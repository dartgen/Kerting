using Kerting_Api.DTO;

namespace Kerting_Api.Interface
{
    /// <summary>
    /// Felhasználói profil modul szerződése.
    /// </summary>
    public interface IUserProfileService
    {
        /// <summary>
        /// Saját profil frissítése mező- és címke-szinten.
        /// </summary>
        Task<UserProfileDto> UpdateMyProfileAsync(int userId, UserProfileDto updatedUser);

        /// <summary>
        /// Bejelentkezett user profilja token alapján.
        /// </summary>
        Task<UserProfileDto> GetMyProfileAsync(int userId);

        /// <summary>
        /// Rendszerben elérhető szerepkörök listája.
        /// </summary>
        Task<List<RoleDto>> GetRolesAsync();

        /// <summary>
        /// Felhasználó kereső nevekre/username-re.
        /// </summary>
        Task<List<UserSearchResultDto>> SearchUsersAsync(string query);

        /// <summary>
        /// Publikus profil lekérése adatvédelmi maszkolási szabályokkal.
        /// </summary>
        Task<PublicProfileDto> GetPublicProfileAsync(int id);
    }
}