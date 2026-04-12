using Kerting_Api.DTO;
using Kerting_Api.Interface;
using Libary;
using Libary.Model.User;
using Microsoft.EntityFrameworkCore;

namespace Kerting_Api.Service
{
    /// <summary>
    /// Kiemelt felhasználó carousel üzleti logikája.
    /// Kezeli a publikus megjelenítési listát és az admin slot-kiosztást.
    /// </summary>
    public class FeaturedUsersService : IFeaturedUsersService
    {
        private readonly KertingDbContext _context;

        public FeaturedUsersService(KertingDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Publikus carousel lista előállítása slot sorrendben.
        /// A hiányos profilokat (név vagy rólam nélkül) kiszűri a stabil UI miatt.
        /// </summary>
        public async Task<List<FeaturedCarouselItemDto>> GetFeaturedUsersForCarouselAsync()
        {
            var slots = await _context.FeaturedUserSlot
                .AsNoTracking()
                .OrderBy(x => x.SlotNo)
                .ToListAsync();

            if (slots.Count == 0)
            {
                return new List<FeaturedCarouselItemDto>();
            }

            var userIds = slots.Select(s => s.UserId).Distinct().ToList();

            var users = await _context.User
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var reviewStats = await _context.UserReview
                .AsNoTracking()
                .Where(r => userIds.Contains(r.TargetUserId) && r.ParentReviewId == null && !r.IsDeleted && r.Rating != null)
                .GroupBy(r => r.TargetUserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Count = g.Count(),
                    Average = Math.Round(g.Average(x => (double)x.Rating!.Value), 1)
                })
                .ToDictionaryAsync(x => x.UserId);

            var result = new List<FeaturedCarouselItemDto>();

            // Slotonként építjük fel a megjelenítési elemeket a kapcsolódó user + review adatokkal.
            foreach (var slot in slots)
            {
                if (!users.TryGetValue(slot.UserId, out var user))
                {
                    continue;
                }

                var fullName = $"{user.VezetekNev} {user.KeresztNev}".Trim();
                if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(user.Rolam))
                {
                    continue;
                }

                var ertekeles = 0d;
                var ertekelesSzam = 0;
                if (reviewStats.TryGetValue(user.Id, out var value))
                {
                    ertekeles = value.Average;
                    ertekelesSzam = value.Count;
                }

                result.Add(new FeaturedCarouselItemDto
                {
                    SlotNo = slot.SlotNo,
                    UserId = user.Id,
                    Name = fullName,
                    Bio = user.Rolam!,
                    ImgString = user.IMGString,
                    Ertekeles = ertekeles,
                    ErtekelesSzam = ertekelesSzam
                });
            }

            return result;
        }

        /// <summary>
        /// Admin oldal adatforrása: aktuális slot-kiosztás + választható felhasználók.
        /// </summary>
        public async Task<AdminFeaturedDataDto> GetAdminFeaturedDataAsync(int userId)
        {
            if (!await IsAdminAsync(userId))
            {
                throw new UnauthorizedAccessException();
            }

            var slots = await _context.FeaturedUserSlot
                .AsNoTracking()
                .OrderBy(x => x.SlotNo)
                .Select(x => new AdminFeaturedSlotDto
                {
                    SlotNo = x.SlotNo,
                    UserId = x.UserId
                })
                .ToListAsync();

            var users = await _context.User
                .AsNoTracking()
                .Where(x => (x.VezetekNev != null && x.VezetekNev != "") || (x.KeresztNev != null && x.KeresztNev != ""))
                .OrderBy(x => x.VezetekNev)
                .ThenBy(x => x.KeresztNev)
                .Select(x => new AdminFeaturedUserOptionDto
                {
                    Id = x.Id,
                    Name = ($"{x.VezetekNev} {x.KeresztNev}").Trim()
                })
                .ToListAsync();

            return new AdminFeaturedDataDto
            {
                Slots = slots,
                Users = users
            };
        }

        /// <summary>
        /// Kiemelt slotok mentése admin ellenőrzéssel és szigorú validációval.
        /// Elvárt: pontosan 5 különböző slot (1..5), különböző user azonosítókkal.
        /// </summary>
        public async Task UpsertFeaturedSlotsAsync(int userId, List<FeaturedSlotUpsertDto> request)
        {
            if (!await IsAdminAsync(userId))
            {
                throw new UnauthorizedAccessException();
            }

            if (request == null || request.Count != 5)
            {
                throw new InvalidOperationException("Pontosan 5 slot beküldése kötelező.");
            }

            var slotNumbers = request.Select(x => x.SlotNo).ToList();
            if (slotNumbers.Distinct().Count() != 5 || slotNumbers.Any(s => s < 1 || s > 5))
            {
                throw new InvalidOperationException("A slot számoknak 1 és 5 közöttinek, valamint egyedinek kell lenniük.");
            }

            var userIds = request.Select(x => x.UserId).ToList();
            if (userIds.Distinct().Count() != 5)
            {
                throw new InvalidOperationException("Egy felhasználó csak egy slotban szerepelhet.");
            }

            var existingUsersCount = await _context.User.CountAsync(x => userIds.Contains(x.Id));
            if (existingUsersCount != 5)
            {
                throw new InvalidOperationException("A beküldött felhasználók között érvénytelen azonosító található.");
            }

            var currentSlots = await _context.FeaturedUserSlot
                .Where(x => slotNumbers.Contains(x.SlotNo))
                .ToDictionaryAsync(x => x.SlotNo);

            foreach (var item in request)
            {
                if (currentSlots.TryGetValue(item.SlotNo, out var slot))
                {
                    slot.UserId = item.UserId;
                    slot.UpdatedAtUtc = DateTime.UtcNow;
                }
                else
                {
                    _context.FeaturedUserSlot.Add(new FeaturedUserSlot
                    {
                        SlotNo = item.SlotNo,
                        UserId = item.UserId,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        // Belső admin jogosultság-ellenőrző segédfüggvény.
        private async Task<bool> IsAdminAsync(int userId)
        {
            var user = await _context.User.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
            return user?.RoleId == 1;
        }
    }
}