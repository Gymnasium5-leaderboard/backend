using Leaderboard.Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Leaderboard.DAL.Interceptors;

public class DateInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = new())
    {
        SetDates(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SetDates(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private static void SetDates(DbContext? dbContext)
    {
        if (dbContext == null) return;

        foreach (var entry in dbContext.ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
                entry.Property(x => x.CreatedAt).CurrentValue = DateTime.UtcNow;

            entry.Property(x => x.LastModifiedAt).CurrentValue = DateTime.UtcNow;
        }
    }
}