using System.Linq.Expressions;
using EvalSystem.Domain.Common;
using EvalSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EvalSystem.Infrastructure.Persistence;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly EvalSystemDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(EvalSystemDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
        => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.FirstOrDefaultAsync(predicate);

    public async Task<bool> ExistsAsync(Guid id)
        => await _dbSet.AnyAsync(e => e.Id == id);

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        => predicate is null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public void Update(T entity)
    {
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void SoftDelete(T entity)
    {
        entity.IsDeleted = true;
        Update(entity);
    }

    public IQueryable<T> Query() => _dbSet.AsQueryable();
}
