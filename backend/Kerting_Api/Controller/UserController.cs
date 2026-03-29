using Libary;
using Libary.Model.Tag;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kerting_Api.Controller
{
    [Route("api")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly KertingDbContext _context;

        public UserController(KertingDbContext context)
        {
            _context = context;
        }

        [Authorize] // Kötelező a bejelentkezés
        [HttpPut("UpdateMyProfile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] Libary.Model.User.User updatedUser)
        {
            // 1. Kiszedjük a bejelentkezett felhasználó ID-ját a Tokenből (Claims)
            var userIdString = User.FindFirst("Id")?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int loggedInUserId))
            {
                return Unauthorized("Érvénytelen token vagy hiányzó azonosító.");
            }

            // 2. KIKERESÉS A TOKENBŐL KAPOTT ID ALAPJÁN (Ezt már nem tudja meghamisítani)
            var existingUser = await _context.User.FindAsync(loggedInUserId);

            if (existingUser == null) // nem lehetséges eset de igy nem kiabál a visual studio
            {
                return NotFound("Felhasználó nem található.");
            }

            // 3. Adatok felülírása
            existingUser.VezetekNev = updatedUser.VezetekNev;
            existingUser.KeresztNev = updatedUser.KeresztNev;
            existingUser.Telefon = updatedUser.Telefon;
            existingUser.Email = updatedUser.Email;
            existingUser.Telepules = updatedUser.Telepules;
            existingUser.Rolam = updatedUser.Rolam;
            existingUser.RoleId = updatedUser.RoleId;
            existingUser.Facebook = updatedUser.Facebook;
            existingUser.Instagram = updatedUser.Instagram;
            existingUser.Tiktok = updatedUser.Tiktok;
            existingUser.EmailPublikus = updatedUser.EmailPublikus;
            existingUser.TelefonPublikus = updatedUser.TelefonPublikus;

            if (updatedUser.Cimkek != null)
            {
                var existingConnections = await _context.UserActivityTag
                                                      .Where(uat => uat.USerId == loggedInUserId)
                                                      .ToListAsync();
                _context.UserActivityTag.RemoveRange(existingConnections);

                // B) Végigmegyünk a Vue-tól kapott string listán
                foreach (var cimkeNev in updatedUser.Cimkek)
                {
                    // Tisztítjuk a szöveget (biztos ami biztos)
                    var cleanCimkeNev = cimkeNev.Trim();

                    // Megnézzük, létezik-e már az ActivityTag táblában
                    var tag = await _context.ActivityTag.FirstOrDefaultAsync(t => t.Activity == cleanCimkeNev);

                    // C) Ha még nem létezik, azonnal létrehozzuk!
                    if (tag == null)
                    {
                        tag = new Libary.Model.Tag.ActivityTag { Activity = cleanCimkeNev };
                        _context.ActivityTag.Add(tag);
                        await _context.SaveChangesAsync(); // Itt egyből el is mentjük, hogy kapjon egy új Id-t az adatbázistól
                    }

                    // D) Létrehozzuk a kapcsolatot az aktuális User és a Tag között
                    var newConnection = new UserActivityTag 
                    {
                        USerId = loggedInUserId,
                        TagId = tag.Id
                    };
                    _context.UserActivityTag.Add(newConnection);
                }
            }


            await _context.SaveChangesAsync();

            return Ok("A profil adatai sikeresen frissítve lettek!");
        }

        [Authorize] // Ide is kötelező a bejelentkezés
        [HttpGet("GetMyProfile")] // Ez egy GET kérés lesz
        public async Task<IActionResult> GetMyProfile()
        {
            // 1. Kiszedjük a bejelentkezett felhasználó ID-ját a Tokenből
            var userIdString = User.FindFirst("Id")?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int loggedInUserId))
            {
                return Unauthorized("Érvénytelen token vagy hiányzó azonosító.");
            }

            // 2. Kikeresjük a felhasználót az adatbázisból
            var userProfile = await _context.User.FindAsync(loggedInUserId);

            if (userProfile == null)
            {
                return NotFound("Felhasználó nem található.");
            }

            // 3. LEKÉRDEZZÜK A FELHASZNÁLÓ CÍMKÉIT (ÚJ RÉSZ)
            // Összekapcsoljuk a UserActivityTag (kapcsoló) és az ActivityTag táblákat
            var userCimkek = await _context.UserActivityTag
                .Where(uat => uat.USerId == loggedInUserId) // Csak a bejelentkezett user sorai
                .Join(
                    _context.ActivityTag, // Melyik táblával kapcsoljuk össze?
                    uat => uat.TagId,     // Kapcsolótábla kulcsa
                    tag => tag.Id,        // ActivityTag tábla kulcsa
                    (uat, tag) => tag.Activity // Mit kérünk ki belőle? (Csak a szöveget!)
                )
                .ToListAsync();

            // 4. Belerakjuk a lekérdezett string listát a User objektumba
            userProfile.Cimkek = userCimkek;

            // 5. Visszaadjuk a teljes User objektumot a frontendnek (JSON formátumban, immár a címkékkel együtt!)
            return Ok(userProfile);
        }

        [Authorize]
        [HttpGet("GetRoles")]
        public async Task<IActionResult> GetRoles()
        {
            // Lekérdezzük az összes szerepkört az adatbázisból
            var roles = await _context.Role.ToListAsync();
            return Ok(roles);
        }
        [AllowAnonymous]
        [HttpGet("GetPublicProfile/{id}")]
        // Ez a végpont publikus, NEM kell rá [Authorize]!
        public async Task<IActionResult> GetPublicProfile(int id)
        {
            // 1. Kikeresjük a felhasználót az Id alapján
            var user = await _context.User.FindAsync(id);
            if (user == null) return NotFound("Felhasználó nem található.");

            // 2. LEKÉRDEZZÜK A CÍMKÉKET
            var userCimkek = await _context.UserActivityTag
                .Where(uat => uat.USerId == id)
                .Join(
                    _context.ActivityTag,
                    uat => uat.TagId,
                    tag => tag.Id,
                    (uat, tag) => tag.Activity
                )
                .ToListAsync();

            // 3. ÉRTÉKELÉSEK KISZÁMOLÁSA (ÚJ RÉSZ)
            // Csak azokat a review-kat nézzük, amiknek VAN csillaga (Rating != null) és nem töröltek
            var reviewsQuery = _context.UserReview
                .Where(r => r.TargetUserId == id && r.ParentReviewId == null && !r.IsDeleted && r.Rating != null);

            var ertekelesSzam = await reviewsQuery.CountAsync();
            double ertekeles = 0;

            if (ertekelesSzam > 0)
            {
                // Átlag kiszámítása
                ertekeles = await reviewsQuery.AverageAsync(r => (double)r.Rating!.Value);
                // Kerekítés 1 tizedesjegyre (opcionális, de szebb a frontendnek)
                ertekeles = Math.Round(ertekeles, 1);
            }

            // --- MASZKOLÁSI LOGIKA KEZDETE ---
            // (Ide jön a te eredeti telefon és email maszkoló kódod változatlanul...)

            string displayTelefon = user.Telefon;
            if (user.TelefonPublikus != true && !string.IsNullOrEmpty(user.Telefon))
            {
                displayTelefon = new string('*', user.Telefon.Length - 4) + user.Telefon.Substring(user.Telefon.Length - 4);
            }

            string displayEmail = user.Email;
            if (user.EmailPublikus != true && !string.IsNullOrEmpty(user.Email))
            {
                int atIndex = user.Email.IndexOf('@');
                string localPart = user.Email.Substring(0, atIndex);
                string domainPart = user.Email.Substring(atIndex);

                if (localPart.Length > 2)
                {
                    displayEmail = localPart.Substring(0, 2) + "***" + domainPart;
                }
                else
                {
                    displayEmail = localPart.Substring(0, 1) + "***" + domainPart;
                }
            }
            // --- MASZKOLÁSI LOGIKA VÉGE ---

            // 4. ADATVÉDELEM ÉS VISSZATÉRÉS
            var publicProfile = new
            {
                user.VezetekNev,
                user.KeresztNev,
                Email = displayEmail,
                Telefon = displayTelefon,
                user.Telepules,
                user.RoleId,
                user.IMGString,
                user.Rolam,
                user.Facebook,
                user.Instagram,
                user.Tiktok,
                user.EmailPublikus,
                user.TelefonPublikus,
                Cimkek = userCimkek,
                Ertekeles = ertekeles,         // <-- ÚJ MEZŐ (Átlag)
                ErtekelesSzam = ertekelesSzam  // <-- ÚJ MEZŐ (Darabszám)
            };

            return Ok(publicProfile);
        }
    }
}
