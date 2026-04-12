using Kerting_Api.Interface;
using Libary.Model.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; // Ez kell ahhoz, hogy ez egy API Controller lehessen (kezeli a HTTP kéréseket).

namespace Kerting_Api.Controller
{   
    // TODO: későbbi biztonsági fejlesztésként érdemes token-frissítési és revokációs stratégiát bevezetni.

    [Route("api")]
    [ApiController]
    /// <summary>
    /// Auth vezérlő: bejelentkezés, regisztráció, first-login és username ellenőrzés végpontok.
    /// A konkrét üzleti logikát az IAuthService kezeli, a controller csak HTTP szintű válaszokat ad.
    /// </summary>
    public class LoginController : ControllerBase
    {
        private readonly IAuthService _authService;

        public LoginController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Bejelentkezés végpont.
        /// Siker esetén JWT tokent ad vissza, hibás hitelesítési adatoknál 401-et.
        /// </summary>
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] DummyLogin loginAdatok)
        {
            try
            {
                var token = await _authService.LoginAsync(loginAdatok);
                return Ok(new
                {
                    token = token,
                    message = "Sikeres bejelentkezés"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Regisztráció végpont.
        /// A backend hash-eli a jelszót, és létrehozza a kapcsolódó user profilt.
        /// </summary>
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] DummyLogin loginAdatok)
        {
            await _authService.RegisterAsync(loginAdatok);
            return Ok("Sikeres regisztráció!");
        }

        /// <summary>
        /// Első belépés állapot kezelés.
        /// A frontend ennek alapján dönt, hogy onboarding/profil szerkesztő nézetre irányít-e.
        /// </summary>
        [Authorize]
        [HttpPost("{id}/first-login")]
        public async Task<IActionResult> HandleFirstLogin(int id)
        {
            try
            {
                var wasFirstLogin = await _authService.HandleFirstLoginAsync(id);
                return Ok(new { isFirstLogin = wasFirstLogin });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Username ellenőrzés regisztráció előtt.
        /// Visszatérés: { isTaken: true/false }.
        /// </summary>
        [HttpGet("CheckUsername")]
        public async Task<IActionResult> CheckUsername([FromQuery] string username)
        {
            var foglalt = await _authService.CheckUsernameAsync(username);
            return Ok(new { isTaken = foglalt });
        }
    }
}