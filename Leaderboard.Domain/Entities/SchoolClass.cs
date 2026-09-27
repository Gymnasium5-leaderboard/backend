using Leaderboard.Domain.Interfaces.Entity;

namespace Leaderboard.Domain.Entities;

public class SchoolClass : IEntityId<long>
{
    public long Id { get; set; }
    public int Grade { get; set; } // 4..11
    public char Letter { get; set; }
    public bool IsActive { get; set; } = true; // false after graduation
    public List<Student> Students { get; set; }

    public string DisplayName => $"{Grade}{Letter}";
}
