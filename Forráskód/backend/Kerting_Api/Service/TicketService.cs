using Kerting_Api.DTO;
using Kerting_Api.Interface;
using Libary;
using Libary.Model;
using Microsoft.EntityFrameworkCore;

namespace Kerting_Api.Service
{
    /// <summary>
    /// Ticket modul üzleti logikája.
    /// </summary>
    public class TicketService : ITicketService
    {
        private readonly KertingDbContext _context;

        public TicketService(KertingDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Új hibajegy létrehozása a bejelentkezett user azonosítójával.
        /// </summary>
        public async Task CreateTicketAsync(int userId, TicketCreateDto dto)
        {
            var ticket = new Ticket
            {
                Title = dto.Title,
                Description = dto.Description,
                UserId = userId,
                CreatedAt = DateTime.Now,
                IsResolved = false
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Összes hibajegy listázása admin felületre.
        /// Nem admin hívónál UnauthorizedAccessException keletkezik.
        /// </summary>
        public async Task<List<TicketListItemDto>> GetAllTicketsAsync(int userId)
        {
            if (!await IsAdminAsync(userId))
            {
                throw new UnauthorizedAccessException("Nincs jogosultságod ehhez a funkcióhoz!");
            }

            return await (from t in _context.Tickets
                          join u in _context.User on t.UserId equals u.Id
                          orderby t.CreatedAt descending
                          select new TicketListItemDto
                          {
                              Id = t.Id,
                              Title = t.Title,
                              Description = t.Description,
                              CreatedAt = t.CreatedAt,
                              IsResolved = t.IsResolved,
                              BekuldoNeve = ((u.VezetekNev ?? string.Empty) + " " + (u.KeresztNev ?? string.Empty)).Trim(),
                              BekuldoAvatar = u.IMGString,
                              UserId = u.Id
                          }).ToListAsync();
        }

        /// <summary>
        /// Hibajegy lezárt állapotba állítása admin jogosultsággal.
        /// </summary>
        public async Task ResolveTicketAsync(int ticketId, int userId)
        {
            if (!await IsAdminAsync(userId))
            {
                throw new UnauthorizedAccessException("Nincs jogosultságod ehhez a funkcióhoz!");
            }

            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
            {
                throw new KeyNotFoundException("A hibajegy nem található.");
            }

            ticket.IsResolved = true;
            await _context.SaveChangesAsync();
        }

        // Belső segédfüggvény az admin szerepkör ellenőrzéséhez.
        private async Task<bool> IsAdminAsync(int userId)
        {
            var currentUser = await _context.User.FindAsync(userId);
            return currentUser?.RoleId == 1;
        }
    }
}