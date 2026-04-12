using Kerting_Api.Interface;
using Libary;
using Microsoft.EntityFrameworkCore;

namespace Kerting_Api.Service
{
    /// <summary>
    /// Aktivitás-címke üzleti logika.
    /// </summary>
    public class ActivityTagService : IActivityTagService
    {
        private readonly KertingDbContext _context;

        public ActivityTagService(KertingDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Visszaadja az összes címke nevét.
        /// A null értékeket kiszűri, hogy a frontend biztosan csak érvényes string listát kapjon.
        /// </summary>
        public async Task<List<string>> GetAllAsync()
        {
            return await _context.ActivityTag
                .Select(x => x.Activity)
                .Where(x => x != null)
                .Select(x => x!)
                .ToListAsync();
        }

            /// <summary>
            /// Címke törlése név alapján.
            /// Csak admin jogosultsággal hajtható végre.
            /// </summary>
        public async Task DeleteByNameAsync(string name, int userId)
        {
            if (!await IsAdminAsync(userId))
            {
                throw new UnauthorizedAccessException("Nincs jogosultságod ehhez a művelethez.");
            }

            var tagToDelete = await _context.ActivityTag
                .FirstOrDefaultAsync(t => t.Activity == name);

            if (tagToDelete == null)
            {
                throw new KeyNotFoundException($"Nem található ilyen nevű tevékenység: {name}");
            }

            _context.ActivityTag.Remove(tagToDelete);
            await _context.SaveChangesAsync();
        }

        // Belső segéd az admin ellenőrzéshez.
        private async Task<bool> IsAdminAsync(int userId)
        {
            var userProfile = await _context.User.FindAsync(userId);
            return userProfile?.RoleId == 1;
        }
    }
}