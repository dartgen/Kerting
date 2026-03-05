namespace Kerting_Api.Interface
{
    public interface GenericInterface<T> where T : class
    {
        Task<List<T>> GetAll();

        Task<T?> GetById(int id);

        Task update(T entity);

        Task Add(T entity);

        Task Delete(int id);
    }
}
