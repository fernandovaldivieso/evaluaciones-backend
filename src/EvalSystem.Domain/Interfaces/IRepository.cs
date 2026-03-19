using System.Linq.Expressions;
using EvalSystem.Domain.Common;

namespace EvalSystem.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Guid id);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<T> AddAsync(T entity);
    void Update(T entity);
    void SoftDelete(T entity);
    IQueryable<T> Query();
}
