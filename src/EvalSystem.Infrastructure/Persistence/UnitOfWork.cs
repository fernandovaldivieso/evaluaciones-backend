using EvalSystem.Domain.Interfaces;

namespace EvalSystem.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly EvalSystemDbContext _context;

    public UnitOfWork(EvalSystemDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
