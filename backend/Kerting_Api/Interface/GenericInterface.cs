namespace Kerting_Api.Interface
{
    /// <summary>
    /// Általános CRUD szerződés, amelyet a generic szolgáltatás valósít meg.
    /// Ezzel a controllerek és modulok egységes API-val érik el az alap adatműveleteket.
    /// </summary>
    public interface GenericInterface<T> where T : class
    {
        /// <summary>
        /// Az összes rekord lekérdezése.
        /// </summary>
        Task<List<T>> GetAll();

        /// <summary>
        /// Egy rekord lekérdezése elsődleges kulcs alapján.
        /// </summary>
        Task<T?> GetById(int id);

        /// <summary>
        /// Meglévő rekord frissítése.
        /// </summary>
        Task update(T entity);

        /// <summary>
        /// Új rekord létrehozása.
        /// </summary>
        Task Add(T entity);

        /// <summary>
        /// Rekord törlése azonosító alapján.
        /// </summary>
        Task Delete(int id);
    }
}
