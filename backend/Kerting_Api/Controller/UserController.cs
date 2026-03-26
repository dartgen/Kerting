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
        [HttpPut("UpdateMyProfile")] // Figyeld meg: kivettem az {id}-t az URL-ből!
        public async Task<IActionResult> UpdateMyProfile([FromBody] Libary.Model.User.User updatedUser)
        {
            // 1. Kiszedjük a bejelentkezett felhasználó ID-ját a Tokenből (Claims)
            // A ClaimTypes.NameIdentifier az a szabványos hely, ahova az ID-t rakni szoktuk a token generálásakor
            var userIdString = User.FindFirst("Id")?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int loggedInUserId))
            {
                return Unauthorized("Érvénytelen token vagy hiányzó azonosító.");
            }

            // 2. KIKERESÉS A TOKENBŐL KAPOTT ID ALAPJÁN (Ezt már nem tudja meghamisítani)
            var existingUser = await _context.User.FindAsync(loggedInUserId);

            if (existingUser == null)
            {
                return NotFound("Felhasználó nem található.");
            }

            // 3. Adatok felülírása (Ugyanaz, mint eddig)
            existingUser.VezetekNev = updatedUser.VezetekNev;
            existingUser.KeresztNev = updatedUser.KeresztNev;
            existingUser.Telefon = updatedUser.Telefon;
            existingUser.Email = updatedUser.Email;
            existingUser.Telepules = updatedUser.Telepules;
            existingUser.Rolam = updatedUser.Rolam;
            existingUser.RoleId = updatedUser.RoleId;

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

        [HttpGet("GetPublicProfile/{id}")]
        // Ez a végpont publikus, NEM kell rá [Authorize]!
        public async Task<IActionResult> GetPublicProfile(int id)
        {
            // 1. Kikeresjük a felhasználót az Id alapján
            var user = await _context.User.FindAsync(id);

            if (user == null)
            {
                return NotFound("A felhasználó nem található.");
            }

            // 2. LEKÉRDEZZÜK A CÍMKÉKET (Pontosan ugyanúgy, mint a GetMyProfile-nál)
            var userCimkek = await _context.UserActivityTag
                .Where(uat => uat.USerId == id)
                .Join(
                    _context.ActivityTag,
                    uat => uat.TagId,
                    tag => tag.Id,
                    (uat, tag) => tag.Activity
                )
                .ToListAsync();

            // 3. ADATVÉDELEM (Döntsd el, mit mutatsz meg!)
            // Csinálunk egy névtelen objektumot (vagy egy PublicUserDTO-t), 
            // amibe csak a biztonságos adatokat tesszük bele.

            var publicProfile = new
            {
                user.VezetekNev,
                user.KeresztNev,
                // Döntés: Megmutatjuk a teljes emailt publikusan?
                // Ha nem, maszkolhatod itt: user.Email.Substring(0, 1) + "***@..."
                user.Email,
                user.Telefon, // Ugyanaz a kérdés, maszkolhatod itt.
                user.Telepules,
                user.RoleId,
                user.IMGString,
                user.Rolam,
                Cimkek = userCimkek // A lekérdezett string lista
            };

            return Ok(publicProfile);
        }
    }
}
