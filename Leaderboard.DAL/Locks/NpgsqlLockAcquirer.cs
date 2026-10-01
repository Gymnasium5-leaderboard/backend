using Microsoft.EntityFrameworkCore;

namespace Leaderboard.DAL.Locks;

/// <summary>
///     PostgreSQL transaction-level advisory locks.
/// </summary>
public class NpgsqlLockAcquirer(ApplicationDbContext context) : ILockAcquirer
{
    public async Task AcquireAsync(IReadOnlyCollection<long> keys, CancellationToken cancellationToken = default)
    {
        // A transaction-level lock outside a transaction is released at once and protects nothing
        if (context.Database.CurrentTransaction == null)
            throw new InvalidOperationException("A lock requires an active transaction");

        // Always in ascending order, so two callers cannot deadlock
        foreach (var key in keys.Distinct().Order())
            await context.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({key})", cancellationToken);
    }
}