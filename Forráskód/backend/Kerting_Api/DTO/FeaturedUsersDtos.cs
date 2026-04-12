namespace Kerting_Api.DTO
{
    /// <summary>
    /// Kiemelt felhasználó carousel kártya DTO a nyilvános kezdőoldalhoz.
    /// </summary>
    public sealed class FeaturedCarouselItemDto
    {
        public byte SlotNo { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string? ImgString { get; set; }
        public double Ertekeles { get; set; }
        public int ErtekelesSzam { get; set; }
    }

    /// <summary>
    /// Admin slot kiosztási rekord: melyik kiemelt helyre melyik user került.
    /// </summary>
    public sealed class AdminFeaturedSlotDto
    {
        public byte SlotNo { get; set; }
        public int UserId { get; set; }
    }

    /// <summary>
    /// Admin nézethez minimális felhasználó opció (id + név).
    /// </summary>
    public sealed class AdminFeaturedUserOptionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Admin felület összesített válasza: aktuális slotok és választható userek.
    /// </summary>
    public sealed class AdminFeaturedDataDto
    {
        public List<AdminFeaturedSlotDto> Slots { get; set; } = new();
        public List<AdminFeaturedUserOptionDto> Users { get; set; } = new();
    }

    /// <summary>
    /// Egy slot felülírásához/létrehozásához szükséges upsert modell.
    /// </summary>
    public sealed class FeaturedSlotUpsertDto
    {
        public byte SlotNo { get; set; }
        public int UserId { get; set; }
    }
}