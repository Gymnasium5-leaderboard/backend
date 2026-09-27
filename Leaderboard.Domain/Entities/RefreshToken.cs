using Leaderboard.Domain.Interfaces.Entity;

namespace Leaderboard.Domain.Entities;

public class RefreshToken : IEntityId<long>
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public LeaderboardOwner Owner { get; set; }
    public string TokenHash { get; set; } 
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
