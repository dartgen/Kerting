namespace Kerting_Api.Interface
{
    /// <summary>
    /// Aktivitás-címke (tag) modul szerződése.
    /// </summary>
    public interface IActivityTagService
    {
        /// <summary>
        /// Az összes címke neve listaként.
        /// </summary>
        Task<List<string>> GetAllAsync();

        /// <summary>
        /// Címke törlése név alapján admin jogosultság ellenőrzéssel.
        /// </summary>
        Task DeleteByNameAsync(string name, int userId);
    }
}