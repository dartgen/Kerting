using Kerting_Api.Interface;
using Libary;
using Libary.Model.Project;
using Microsoft.EntityFrameworkCore;

namespace Kerting_Api.Service
{
    /// <summary>
    /// Személyes naptárbejegyzések üzleti logikája.
    /// </summary>
    public class CalendarService : ICalendarService
    {
        private readonly KertingDbContext _context;

        public CalendarService(KertingDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Userhez tartozó bejegyzések dátum szerinti listázása.
        /// </summary>
        public async Task<IEnumerable<CalendarEntry>> GetEntriesByUserIdAsync(string userId)
        {
            return await _context.CalendarEntries
                .Where(e => e.UserId == userId)
                .OrderBy(e => e.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Új bejegyzés létrehozása, vagy létező bejegyzés frissítése.
        /// </summary>
        public async Task<CalendarEntry> CreateEntryAsync(CalendarEntry entry)
        {
            if (entry.Id > 0)
            {
                _context.CalendarEntries.Update(entry);
            }
            else
            {
                _context.CalendarEntries.Add(entry);
            }
            await _context.SaveChangesAsync();
            return entry;
        }

        /// <summary>
        /// Bejegyzés törlése, de csak ha a megadott userhez tartozik.
        /// </summary>
        public async Task DeleteEntryAsync(int entryId, string userId)
        {
            var entry = await _context.CalendarEntries
                .FirstOrDefaultAsync(e => e.Id == entryId && e.UserId == userId);

            if (entry != null)
            {
                _context.CalendarEntries.Remove(entry);
                await _context.SaveChangesAsync();
            }
        }
    }
}