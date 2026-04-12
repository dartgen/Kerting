namespace Kerting_Api.DTO
{
    /// <summary>
    /// Ticket lista nézet egy sorának DTO-ja admin/felhasználói listázáshoz.
    /// </summary>
    public sealed class TicketListItemDto
    {
        /// <summary>
        /// Ticket azonosító.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Ticket cím.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Ticket leírás.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Létrehozás időpontja.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Megoldottsági állapot.
        /// </summary>
        public bool IsResolved { get; set; }

        /// <summary>
        /// Beküldő megjelenített neve.
        /// </summary>
        public string BekuldoNeve { get; set; } = string.Empty;

        /// <summary>
        /// Beküldő avatar fájlneve/útvonala.
        /// </summary>
        public string? BekuldoAvatar { get; set; }

        /// <summary>
        /// Beküldő user azonosító.
        /// </summary>
        public int UserId { get; set; }
    }
}