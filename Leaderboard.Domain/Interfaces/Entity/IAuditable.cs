namespace Leaderboard.Domain.Interfaces.Entity;

public interface IAuditable
{
    public DateTime CreatedAt { get; set; }

    public DateTime? LastModifiedAt { get; set; }
}
