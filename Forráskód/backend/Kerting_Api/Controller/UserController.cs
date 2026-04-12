using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Kerting_Api.DTO;
using Kerting_Api.Interface;

namespace Kerting_Api.Controller
{
    [Route("api")]
    [ApiController]
    /// <summary>
    /// Felhasználói profil végpontok: saját profil kezelés, szerepkörlista, keresés, publikus profil.
    /// </summary>
    public class UserController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;

        public UserController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        // Segédfüggvény a tokenben tárolt felhasználó azonosító biztonságos olvasására.
        private int? GetCurrentUserId()
        {
            var userIdString = User.FindFirst("Id")?.Value;
            return int.TryParse(userIdString, out var userId) ? userId : null;
        }

        /// <summary>
        /// A bejelentkezett user saját profiladatainak frissítése.
        /// </summary>
        [Authorize] // Kötelező a bejelentkezés
        [HttpPut("UpdateMyProfile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UserProfileDto updatedUser)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized("Érvénytelen token vagy hiányzó azonosító.");
            }

            try
            {
                await _userProfileService.UpdateMyProfileAsync(userId.Value, updatedUser);
                return Ok("A profil adatai sikeresen frissítve lettek!");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// A bejelentkezett user saját profiljának lekérése.
        /// </summary>
        [Authorize] // Ide is kötelező a bejelentkezés
        [HttpGet("GetMyProfile")] // Ez egy GET kérés lesz
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized("Érvénytelen token vagy hiányzó azonosító.");
            }

            try
            {
                var profile = await _userProfileService.GetMyProfileAsync(userId.Value);
                return Ok(profile);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Rendszerben elérhető szerepkörök listája.
        /// </summary>
        [Authorize]
        [HttpGet("GetRoles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _userProfileService.GetRolesAsync();
            return Ok(roles);
        }

        /// <summary>
        /// Felhasználók keresése névrészlet alapján.
        /// </summary>
        [Authorize]
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string q)
        {
            var users = await _userProfileService.SearchUsersAsync(q);
            return Ok(users);
        }

        /// <summary>
        /// Publikus profil lekérése anonimen is elérhető módon.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("GetPublicProfile/{id}")]
        public async Task<IActionResult> GetPublicProfile(int id)
        {
            try
            {
                var profile = await _userProfileService.GetPublicProfileAsync(id);
                return Ok(profile);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}