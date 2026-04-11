using Libary.Model.Project;

namespace Kerting_Api.Interface
{
    public interface ICalendarService
    {
        Task<IEnumerable<CalendarEntry>> GetEntriesByUserIdAsync(string userId);
        Task<CalendarEntry> CreateEntryAsync(CalendarEntry entry);
        Task DeleteEntryAsync(int entryId, string userId);
    }
}