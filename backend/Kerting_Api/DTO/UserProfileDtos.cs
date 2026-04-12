namespace Kerting_Api.DTO
{
    /// <summary>
    /// Saját profil lekérdezés/szerkesztés során használt teljes profil DTO.
    /// </summary>
    public sealed class UserProfileDto
    {
        public int Id { get; set; }
        public string? VezetekNev { get; set; }
        public string? KeresztNev { get; set; }
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public string? Telepules { get; set; }
        public int RoleId { get; set; }
        public string? IMGString { get; set; }
        public string? Rolam { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? Tiktok { get; set; }

        // Ezek a kapcsolók határozzák meg, hogy nyilvános nézetben látható-e az email/telefon.
        public bool EmailPublikus { get; set; }
        public bool TelefonPublikus { get; set; }
        public string? RoleName { get; set; }

        // Tevékenységi címkék listája profil és keresés támogatására.
        public List<string> Cimkek { get; set; } = new();
        public string? Username { get; set; }
    }

    /// <summary>
    /// Nyilvános profil DTO, amely csak a publikusan vállalt adatokat és értékelési mutatókat tartalmazza.
    /// </summary>
    public sealed class PublicProfileDto
    {
        public string? VezetekNev { get; set; }
        public string? KeresztNev { get; set; }
        public string? Email { get; set; }
        public string? Telefon { get; set; }
        public string? Telepules { get; set; }
        public int RoleId { get; set; }
        public string? IMGString { get; set; }
        public string? Rolam { get; set; }
        public string? RoleName { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? Tiktok { get; set; }
        public bool EmailPublikus { get; set; }
        public bool TelefonPublikus { get; set; }
        public List<string> Cimkek { get; set; } = new();
        public double Ertekeles { get; set; }
        public int ErtekelesSzam { get; set; }
        public string? Username { get; set; }
    }

    /// <summary>
    /// Felhasználó kereső találat rövidített DTO-ja (pl. csapattag választóban).
    /// </summary>
    public sealed class UserSearchResultDto
    {
        public string Id { get; set; } = string.Empty;
        public string Nev { get; set; } = string.Empty;
        public string Szakma { get; set; } = string.Empty;
        public string? Avatar { get; set; }
    }

    /// <summary>
    /// Egyszerű szerepkör DTO listákhoz és szűrőkhöz.
    /// </summary>
    public sealed class RoleDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}