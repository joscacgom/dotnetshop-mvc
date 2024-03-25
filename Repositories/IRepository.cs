using System.Linq.Expressions;

namespace dotnetshop.Repositories
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
        T? GetById(int id);
        T? Get(Expression<Func<T, bool>> filter, string? includeProperties = null);
        void Insert(T entity);
        void Update(T entity);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
        void Save();
    }
}