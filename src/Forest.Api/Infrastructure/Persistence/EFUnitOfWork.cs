using Forest.Application.Abstractions;

namespace Forest.Infrastructure.Persistence;

public class EFUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public EFUnitOfWork(AppDbContext db)
    {
        _db = db;
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try {
            await action(ct);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try {
            var result = await action(ct);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return result;
        }
        catch {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
