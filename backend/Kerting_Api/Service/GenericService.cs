using Libary;
using Microsoft.EntityFrameworkCore;

namespace Kerting_Api.Service
{
    /// <summary>
    /// Egyszerű, újrahasználható CRUD szolgáltatás bármely EF entitásra.
    /// Az osztály célja, hogy az alapműveleteket ne kelljen minden modulban újraírni.
    /// </summary>
    public class GenericService<T> : Interface.GenericInterface<T> where T : class
    {
        private readonly KertingDbContext _context;
        private readonly DbSet<T> _set;

        public GenericService(KertingDbContext context)
        {
            _context = context;
            _set = context.Set<T>();
        }

        /// <summary>
        /// Visszaadja az adott entitás összes rekordját.
        /// </summary>
        public async Task<List<T>> GetAll() => await _set.ToListAsync();

        /// <summary>
        /// Elsődleges kulcs alapján megkeresi az entitást.
        /// Ha nincs találat, null értékkel tér vissza.
        /// </summary>
        public async Task<T?> GetById(int id) => await _set.FindAsync(id);

        /// <summary>
        /// Új entitás mentése adatbázisba.
        /// </summary>
        public async Task Add(T entity)
        {
            _set.Add(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Entitás törlése azonosító alapján.
        /// Ha a rekord nem létezik, csendben visszatér (nincs kivétel).
        /// </summary>
        public async Task Delete(int id)
        {
            var entity = await GetById(id);
            if (entity == null)
            {
                return;
            }
            _set.Remove(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Létező entitás módosítása.
        /// Fontos: a hívónak kell biztosítania, hogy az entity megfelelő állapotban legyen.
        /// </summary>
        public async Task update(T entity)
        {
            _set.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}

