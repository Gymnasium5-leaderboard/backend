using Leaderboard.Domain.Interfaces.Entity;

namespace Leaderboard.Domain.Entities;

// Append-only ledger: the single source of truth for scores
public class ScoreTransaction : IEntityId<long>
{
    public long AcademicYearId { get; set; }
    public AcademicYear AcademicYear { get; set; }
    public long StudentId { get; set; }
    public Student Student { get; set; }
    public long OwnerId { get; set; } // who changed the score
    public LeaderboardOwner Owner { get; set; }
    public int Delta { get; set; }
    public string? Description { get; set; }
    public Guid? IdempotencyKey { get; set; }
    public DateTime CreatedAt { get; set; } // set by the backend
    public long Id { get; set; }
}