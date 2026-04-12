using Libary.Model.Project;

namespace Kerting_Api.Interface
{
    /// <summary>
    /// Személyes naptár bejegyzések service szerződése.
    /// </summary>
    public interface ICalendarService
    {
        /// <summary>
        /// Userhez tartozó bejegyzések listázása.
        /// </summary>
        Task<IEnumerable<CalendarEntry>> GetEntriesByUserIdAsync(string userId);

        /// <summary>
        /// Bejegyzés létrehozása vagy frissítése.
        /// </summary>
        Task<CalendarEntry> CreateEntryAsync(CalendarEntry entry);

        /// <summary>
        /// User saját bejegyzésének törlése.
        /// </summary>
        Task DeleteEntryAsync(int entryId, string userId);
    }
}