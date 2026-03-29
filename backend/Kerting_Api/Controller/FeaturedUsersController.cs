using Libary;
using Libary.Model.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kerting_Api.Controller
{
    [Route("api/featured-users")]
    [ApiController]
    public class FeaturedUsersController : ControllerBase
    {
        private readonly KertingDbContext _context;

        public FeaturedUsersController(KertingDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetFeaturedUsersForCarousel()
        {
            var slots = await _context.FeaturedUserSlot
                .AsNoTracking()
                .OrderBy(x => x.SlotNo)
                .ToListAsync();

            if (slots.Count == 0)
            {
                return Ok(new List<FeaturedCarouselItemDto>());
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

            return Ok(result);
        }

        [Authorize]
        [HttpGet("admin/data")]
        public async Task<IActionResult> GetAdminFeaturedData()
        {
            if (!await IsAdminAsync())
            {
                return Forbid();
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

            return Ok(new AdminFeaturedDataDto
            {
                Slots = slots,
                Users = users
            });
        }

        [Authorize]
        [HttpPut("admin/slots")]
        public async Task<IActionResult> UpsertFeaturedSlots([FromBody] List<FeaturedSlotUpsertDto>? request)
        {
            if (!await IsAdminAsync())
            {
                return Forbid();
            }

            if (request == null || request.Count != 5)
            {
                return BadRequest("Pontosan 5 slot beküldése kötelező.");
            }

            var slotNumbers = request.Select(x => x.SlotNo).ToList();
            if (slotNumbers.Distinct().Count() != 5 || slotNumbers.Any(s => s < 1 || s > 5))
            {
                return BadRequest("A slot számoknak 1 és 5 közöttinek, valamint egyedinek kell lenniük.");
            }

            var userIds = request.Select(x => x.UserId).ToList();
            if (userIds.Distinct().Count() != 5)
            {
                return BadRequest("Egy felhasználó csak egy slotban szerepelhet.");
            }

            var existingUsersCount = await _context.User.CountAsync(x => userIds.Contains(x.Id));
            if (existingUsersCount != 5)
            {
                return BadRequest("A beküldött felhasználók között érvénytelen azonosító található.");
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
            return Ok(new { Message = "Kiemelt felhasználók sikeresen frissítve." });
        }

        private async Task<bool> IsAdminAsync()
        {
            var userIdString = User.FindFirst("Id")?.Value;
            if (!int.TryParse(userIdString, out var userId))
            {
                return false;
            }

            var user = await _context.User.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
            return user?.RoleId == 1;
        }

        public sealed class FeaturedCarouselItemDto
        {
            public byte SlotNo { get; set; }
            public int UserId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Bio { get; set; } = string.Empty;
            public string? ImgString { get; set; }
            public double Ertekeles { get; set; }
            public int ErtekelesSzam { get; set; }
        }

        public sealed class AdminFeaturedSlotDto
        {
            public byte SlotNo { get; set; }
            public int UserId { get; set; }
        }

        public sealed class AdminFeaturedUserOptionDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public sealed class AdminFeaturedDataDto
        {
            public List<AdminFeaturedSlotDto> Slots { get; set; } = new();
            public List<AdminFeaturedUserOptionDto> Users { get; set; } = new();
        }

        public sealed class FeaturedSlotUpsertDto
        {
            public byte SlotNo { get; set; }
            public int UserId { get; set; }
        }
    }
}
