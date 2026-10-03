using Leaderboard.Domain.Entities;

namespace Leaderboard.Tests.TestData;

internal static class StudentMother
{
    public static IQueryable<Student> GetStudents()
    {
        return new Student[]
        {
            new() { Id = 1, FirstName = "Ivan", LastName = "Petrov", ClassId = 1, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, FirstName = "Anna", LastName = "Sidorova", ClassId = 2, CreatedAt = DateTime.UtcNow },
            new() { Id = 3, FirstName = "Oleg", LastName = "Ivanov", ClassId = 3, CreatedAt = DateTime.UtcNow },
            new() { Id = 4, FirstName = "Maria", LastName = "Smirnova", ClassId = 3, CreatedAt = DateTime.UtcNow },
            new() // left the school
            {
                Id = 5, FirstName = "Petr", LastName = "Kozlov", ClassId = 3, IsActive = false,
                CreatedAt = DateTime.UtcNow
            },
            new() { Id = 6, FirstName = "Elena", LastName = "Volkova", ClassId = 4, CreatedAt = DateTime.UtcNow },
            new() // graduated with class 5
            {
                Id = 7, FirstName = "Dmitry", LastName = "Orlov", ClassId = 5, IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        }.AsQueryable();
    }
}