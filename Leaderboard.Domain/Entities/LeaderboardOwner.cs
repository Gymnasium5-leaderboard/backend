using Leaderboard.Domain.Interfaces.Entity;

namespace Leaderboard.Domain.Entities;

public class LeaderboardOwner : IEntityId<long>, IAuditable
{
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Login { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}
