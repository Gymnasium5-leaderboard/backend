using Leaderboard.Domain.Interfaces.Entity;

namespace Leaderboard.Domain.Entities;

public class Category : IEntityId<long>
{
    public long Id { get; set; }
    public string Name { get; set; }
    public bool IsArchived { get; set; } // archived: not counted anywhere except history
}
