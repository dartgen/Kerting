namespace Kerting_Api.DTO
{
    /// <summary>
    /// Work listaoldal egy rekordja, author és címke összefoglaló adatokkal.
    /// </summary>
    public class WorkListItemDto
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public WorkUserSummaryDto? Author { get; set; }
        public string TargetAudience { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? BasePrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public List<WorkTagLinkDto> Tags { get; set; } = new();
        public bool IsCurrentUserRelated { get; set; }
    }

    /// <summary>
    /// Work listákban használt rövidített szerző/felhasználó nézet.
    /// </summary>
    public class WorkUserSummaryDto
    {
        public int Id { get; set; }
        public string? VezetekNev { get; set; }
        public string? KeresztNev { get; set; }
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public string? Telepules { get; set; }
        public int? RoleId { get; set; }
        public string? ImgString { get; set; }
    }

    /// <summary>
    /// Work és activity címke kapcsolati DTO.
    /// </summary>
    public class WorkTagLinkDto
    {
        public WorkTagActivityDto? Tag { get; set; }
    }

    /// <summary>
    /// Címke aktivitás nevét hordozó minimál DTO.
    /// </summary>
    public class WorkTagActivityDto
    {
        public string? Activity { get; set; }
    }
}