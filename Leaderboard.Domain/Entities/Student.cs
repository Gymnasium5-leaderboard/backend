using Leaderboard.Domain.Interfaces.Entity;

namespace Leaderboard.Domain.Entities;

public class Student : IEntityId<long>, IAuditable
{
    public long Id { get; set; }
    public string FirstName { get; set; } 
    public string LastName { get; set; } 
    public long ClassId { get; set; } // current class, points follow the student
    public SchoolClass Class { get; set; } 
    public bool IsActive { get; set; } = true; // false if the student left the school
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}
