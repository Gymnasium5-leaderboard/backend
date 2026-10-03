using Leaderboard.Domain.Entities;

namespace Leaderboard.Tests.TestData;

internal static class ClassMother
{
    public static IQueryable<SchoolClass> GetClasses()
    {
        return new SchoolClass[]
        {
            new() { Id = 1, Grade = 11, Letter = 'А' },
            new() { Id = 2, Grade = 10, Letter = 'А' },
            new() { Id = 3, Grade = 7, Letter = 'А' },
            new() { Id = 4, Grade = 7, Letter = 'Б' },
            new() { Id = 5, Grade = 11, Letter = 'Б', IsActive = false }, // graduated
            new() { Id = 6, Grade = 5, Letter = 'В' } // no students
        }.AsQueryable();
    }
}