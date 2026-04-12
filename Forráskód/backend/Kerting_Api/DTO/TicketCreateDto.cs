namespace Kerting_Api.DTO
{
    /// <summary>
    /// Új support ticket létrehozásához szükséges kérésmodell.
    /// </summary>
    public class TicketCreateDto
    {
        /// <summary>
        /// A bejelentés rövid címe.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// A probléma részletes leírása.
        /// </summary>
        public string Description { get; set; }
    }
}
