using Libary.Model.Auth;

namespace Kerting_Api.Interface
{
    /// <summary>
    /// Hitelesítési és alap fiókkezelési szerződés.
    /// A controller ezen az interfészen keresztül éri el az auth üzleti logikát.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Felhasználónév + jelszó ellenőrzése, majd JWT token generálása.
        /// </summary>
        Task<string> LoginAsync(DummyLogin loginAdatok);

        /// <summary>
        /// Új fiók létrehozása hash-elt jelszóval és alapértelmezett szerepkörrel.
        /// </summary>
        Task RegisterAsync(DummyLogin loginAdatok);

        /// <summary>
        /// Első bejelentkezés flag kezelése.
        /// Visszatérési érték: igaz, ha a felhasználó valóban első belépésen volt.
        /// </summary>
        Task<bool> HandleFirstLoginAsync(int id);

        /// <summary>
        /// Ellenőrzi, hogy a kívánt felhasználónév már foglalt-e.
        /// </summary>
        Task<bool> CheckUsernameAsync(string username);
    }
}