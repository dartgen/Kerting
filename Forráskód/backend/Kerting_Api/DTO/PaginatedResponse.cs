namespace Kerting_Api.DTO
{
    /// <summary>
    /// Általános lapozott API válaszmodell lista endpointokhoz.
    /// </summary>
    public class PaginatedResponse<T>
    {
        /// <summary>
        /// Az aktuális oldalon visszaadott elemek.
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// Összes találat száma szűrés után.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Jelenlegi oldalszám (1-től indexelve).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Egy oldalon visszaadott elemek száma.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Teljes oldalszám.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Igaz, ha létezik következő oldal.
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Igaz, ha létezik előző oldal.
        /// </summary>
        public bool HasPreviousPage { get; set; }

        public PaginatedResponse() { }

        /// <summary>
        /// Konstruktor, amely a lapozási metaadatokat automatikusan kiszámolja.
        /// </summary>
        public PaginatedResponse(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            HasNextPage = pageNumber < TotalPages;
            HasPreviousPage = pageNumber > 1;
        }
    }
}
