using Kerting_Api.DTO;

namespace Kerting_Api.Interface
{
    /// <summary>
    /// Hibajegy (ticket) modul üzleti szerződése.
    /// </summary>
    public interface ITicketService
    {
        /// <summary>
        /// Új hibajegy létrehozása a bejelentkezett user nevében.
        /// </summary>
        Task CreateTicketAsync(int userId, TicketCreateDto dto);

        /// <summary>
        /// Összes hibajegy listázása admin jogosultság mellett.
        /// </summary>
        Task<List<TicketListItemDto>> GetAllTicketsAsync(int userId);

        /// <summary>
        /// Hibajegy lezárt állapotba állítása.
        /// </summary>
        Task ResolveTicketAsync(int ticketId, int userId);
    }
}