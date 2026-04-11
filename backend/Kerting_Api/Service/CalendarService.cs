using Kerting_Api.Interface;
using Libary;
using Libary.Model.Project;
using Microsoft.EntityFrameworkCore;

namespace Kerting_Api.Service
{
    public class CalendarService : ICalendarService
    {
        private readonly KertingDbContext _context;

        public CalendarService(KertingDbContext context)
        {
            _context = context;
        }

        // 1. Metódus: Név pontosan egyezik az Interface-szel!
        public async Task<IEnumerable<CalendarEntry>> GetEntriesByUserIdAsync(string userId)
        {
            return await _context.CalendarEntries
                .Where(e => e.UserId == userId)
                .OrderBy(e => e.Date)
                .ToListAsync();
        }

        // 2. Metódus: Név pontosan egyezik az Interface-szel!
        public async Task<CalendarEntry> CreateEntryAsync(CalendarEntry entry)
        {
            if (entry.Id > 0)
            {
                _context.CalendarEntries.Update(entry); // Frissítés, ha már létezik
            }
            else
            {
                _context.CalendarEntries.Add(entry); // Új létrehozása
            }
            await _context.SaveChangesAsync();
            return entry;
        }

        // 3. Metódus: Név pontosan egyezik az Interface-szel!
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