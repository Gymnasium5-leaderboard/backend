using Leaderboard.Domain.Entities;

namespace Leaderboard.Tests.TestData;

internal static class CategoryMother
{
    /// <summary>
    ///     1-5 are the categories seeded by the migration, 6 is archived.
    /// </summary>
    public static IQueryable<Category> GetCategories()
    {
        return new Category[]
        {
            new() { Id = 1, Name = "Учёба" },
            new() { Id = 2, Name = "Спорт" },
            new() { Id = 3, Name = "Творчество" },
            new() { Id = 4, Name = "Общественная деятельность" },
            new() { Id = 5, Name = "Военно-патриотическая деятельность" },
            new() { Id = 6, Name = "Олимпиады", IsArchived = true }
        }.AsQueryable();
    }
}
