using Libary;
using Microsoft.EntityFrameworkCore;

namespace Kerting_Api.Service
{
    public class GenericService<T> : Interface.GenericInterface<T> where T : class
    {
        private readonly KertingDbContext _context;
        private readonly DbSet<T> _set;

        public GenericService(KertingDbContext context)
        {
            _context = context;
            _set = context.Set<T>();
        }

        public async Task<List<T>> GetAll() => await _set.ToListAsync();

        public async Task<T?> GetById(int id) => await _set.FindAsync(id);
        public async Task Add(T entity)
        {
            _set.Add(entity);
            await _context.SaveChangesAsync();
        }

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

        public async Task update(T entity)
        {
            _set.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}

