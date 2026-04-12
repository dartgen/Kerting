using Kerting_Api.DTO;
using Kerting_Api.Interface;
using Libary;
using Libary.Model.Auth;
using Libary.Model.Tag;
using Libary.Model.User;
using Microsoft.EntityFrameworkCore;

namespace Kerting_Api.Service
{
    /// <summary>
    /// Felhasználói profilkezelés üzleti logikája.
    /// Lefedi a saját profil frissítést, publikus profil lekérést,
    /// szerepkör-listát és felhasználó keresést is.
    /// </summary>
    public sealed class UserProfileService : IUserProfileService
    {
        private readonly KertingDbContext _context;

        public UserProfileService(KertingDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Saját profil frissítése + címke-kapcsolatok újraépítése.
        /// A címkéknél automatikusan létrehozza a hiányzó ActivityTag rekordokat.
        /// </summary>
        public async Task<UserProfileDto> UpdateMyProfileAsync(int userId, UserProfileDto updatedUser)
        {
            var existingUser = await _context.User.FindAsync(userId);
            if (existingUser == null)
            {
                throw new KeyNotFoundException("Felhasználó nem található.");
            }

            existingUser.VezetekNev = updatedUser.VezetekNev;
            existingUser.KeresztNev = updatedUser.KeresztNev;
            existingUser.Telefon = updatedUser.Telefon;
            existingUser.Email = updatedUser.Email;
            existingUser.Telepules = updatedUser.Telepules;
            existingUser.Rolam = updatedUser.Rolam;
            existingUser.RoleId = updatedUser.RoleId;
            existingUser.Facebook = updatedUser.Facebook;
            existingUser.Instagram = updatedUser.Instagram;
            existingUser.Tiktok = updatedUser.Tiktok;
            existingUser.EmailPublikus = updatedUser.EmailPublikus;
            existingUser.TelefonPublikus = updatedUser.TelefonPublikus;

            // Címke kapcsolatok teljes újragenerálása,
            // hogy a kliens oldali lista legyen az igazság forrása.
            if (updatedUser.Cimkek != null)
            {
                var existingConnections = await _context.UserActivityTag
                    .Where(uat => uat.USerId == userId)
                    .ToListAsync();
                _context.UserActivityTag.RemoveRange(existingConnections);

                foreach (var cimkeNev in updatedUser.Cimkek)
                {
                    var cleanCimkeNev = cimkeNev.Trim();
                    var tag = await _context.ActivityTag.FirstOrDefaultAsync(t => t.Activity == cleanCimkeNev);

                    if (tag == null)
                    {
                        tag = new ActivityTag { Activity = cleanCimkeNev };
                        _context.ActivityTag.Add(tag);
                        await _context.SaveChangesAsync();
                    }

                    _context.UserActivityTag.Add(new UserActivityTag
                    {
                        USerId = userId,
                        TagId = tag.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
            return updatedUser;
        }

        /// <summary>
        /// Saját profil lekérése szerepkör-névvel, címkékkel és username mezővel kiegészítve.
        /// </summary>
        public async Task<UserProfileDto> GetMyProfileAsync(int userId)
        {
            var userProfile = await _context.User.FindAsync(userId);
            if (userProfile == null)
            {
                throw new KeyNotFoundException("Felhasználó nem található.");
            }

            var userCimkek = await _context.UserActivityTag
                .Where(uat => uat.USerId == userId)
                .Join(
                    _context.ActivityTag,
                    uat => uat.TagId,
                    tag => tag.Id,
                    (uat, tag) => tag.Activity)
                .ToListAsync();

            var username = await _context.Set<Login>()
                .Where(l => l.Id == userId)
                .Select(l => l.Username)
                .FirstOrDefaultAsync();

            var roleName = await _context.Role
                .Where(r => r.Id == userProfile.RoleId)
                .Select(r => r.Name)
                .FirstOrDefaultAsync();

            return new UserProfileDto
            {
                Id = userProfile.Id,
                VezetekNev = userProfile.VezetekNev,
                KeresztNev = userProfile.KeresztNev,
                Telefon = userProfile.Telefon,
                Email = userProfile.Email,
                Telepules = userProfile.Telepules,
                RoleId = userProfile.RoleId,
                IMGString = userProfile.IMGString,
                Rolam = userProfile.Rolam,
                Facebook = userProfile.Facebook,
                Instagram = userProfile.Instagram,
                Tiktok = userProfile.Tiktok,
                EmailPublikus = userProfile.EmailPublikus,
                TelefonPublikus = userProfile.TelefonPublikus,
                RoleName = roleName,
                Cimkek = userCimkek,
                Username = username
            };
        }

        /// <summary>
        /// Szerepkör törzsadatok listázása.
        /// </summary>
        public async Task<List<RoleDto>> GetRolesAsync()
        {
            return await _context.Role
                .Select(r => new RoleDto { Id = r.Id, Name = r.Name })
                .ToListAsync();
        }

            /// <summary>
            /// Felhasználó kereső (név + username), minimum 2 karaktertől.
            /// </summary>
        public async Task<List<UserSearchResultDto>> SearchUsersAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return new List<UserSearchResultDto>();
            }

            return await (from u in _context.User
                          join l in _context.Login on u.Id equals l.Id
                          where u.VezetekNev.Contains(query) ||
                                u.KeresztNev.Contains(query) ||
                                l.Username.Contains(query)
                          select new UserSearchResultDto
                          {
                              Id = u.Id.ToString(),
                              Nev = u.VezetekNev + " " + u.KeresztNev,
                              Szakma = "Felhasználó",
                              Avatar = u.IMGString
                          })
                          .Take(10)
                          .ToListAsync();
        }

        /// <summary>
        /// Publikus profil lekérése adatvédelmi maszkolással.
        /// Email/telefon mezőket a publikus flag-ek szerint maszkolja.
        /// </summary>
        public async Task<PublicProfileDto> GetPublicProfileAsync(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException("Felhasználó nem található.");
            }

            var userCimkek = await _context.UserActivityTag
                .Where(uat => uat.USerId == id)
                .Join(
                    _context.ActivityTag,
                    uat => uat.TagId,
                    tag => tag.Id,
                    (uat, tag) => tag.Activity)
                .ToListAsync();

            var reviewsQuery = _context.UserReview
                .Where(r => r.TargetUserId == id && r.ParentReviewId == null && !r.IsDeleted && r.Rating != null);

            var ertekelesSzam = await reviewsQuery.CountAsync();
            double ertekeles = 0;
            if (ertekelesSzam > 0)
            {
                ertekeles = await reviewsQuery.AverageAsync(r => (double)r.Rating!.Value);
                ertekeles = Math.Round(ertekeles, 1);
            }

            // Telefon maszkolás, ha nem publikus.
            var displayTelefon = user.Telefon;
            if (user.TelefonPublikus != true && !string.IsNullOrEmpty(user.Telefon) && user.Telefon.Length > 4)
            {
                displayTelefon = new string('*', user.Telefon.Length - 4) + user.Telefon.Substring(user.Telefon.Length - 4);
            }

            // Email maszkolás, ha nem publikus.
            var displayEmail = user.Email;
            if (user.EmailPublikus != true && !string.IsNullOrEmpty(user.Email))
            {
                var atIndex = user.Email.IndexOf('@');
                if (atIndex > 0)
                {
                    var localPart = user.Email.Substring(0, atIndex);
                    var domainPart = user.Email.Substring(atIndex);
                    displayEmail = localPart.Length > 2
                        ? localPart.Substring(0, 2) + "***" + domainPart
                        : localPart.Substring(0, 1) + "***" + domainPart;
                }
            }

            var username = await _context.Set<Login>()
                .Where(l => l.Id == id)
                .Select(l => l.Username)
                .FirstOrDefaultAsync();

            var roleName = await _context.Role
                .Where(r => r.Id == user.RoleId)
                .Select(r => r.Name)
                .FirstOrDefaultAsync();

            return new PublicProfileDto
            {
                VezetekNev = user.VezetekNev,
                KeresztNev = user.KeresztNev,
                Email = displayEmail,
                Telefon = displayTelefon,
                Telepules = user.Telepules,
                RoleId = user.RoleId,
                IMGString = user.IMGString,
                Rolam = user.Rolam,
                RoleName = roleName,
                Facebook = user.Facebook,
                Instagram = user.Instagram,
                Tiktok = user.Tiktok,
                EmailPublikus = user.EmailPublikus,
                TelefonPublikus = user.TelefonPublikus,
                Cimkek = userCimkek,
                Ertekeles = ertekeles,
                ErtekelesSzam = ertekelesSzam,
                Username = username
            };
        }
    }
}