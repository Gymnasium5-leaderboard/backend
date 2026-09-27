using Leaderboard.Domain.Interfaces.Entity;

namespace Leaderboard.Domain.Entities;

public class AcademicYear : IEntityId<long>
{
    public long Id { get; set; }
    public string Title { get; set; }  // "2026/2027"
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; } // null = current year

    public bool IsCurrent => FinishedAt == null;
}
