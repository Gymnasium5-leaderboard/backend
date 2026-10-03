using Leaderboard.Domain.Entities;
using Leaderboard.Tests.Constants;

namespace Leaderboard.Tests.TestData;

internal static class ScoreTransactionMother
{
    /// <summary>
    ///     Scores of the current year: student 1 = 5, 2 = 3, 3 = 10, 4 = 10 (reached after 3), 5 = 50 (inactive), 6 = 6.
    ///     Class averages: 7А = 10, 7Б = 6, 11А = 5, 10А = 3.
    /// </summary>
    public static IQueryable<ScoreTransaction> GetScoreTransactions()
    {
        return new ScoreTransaction[]
        {
            new()
            {
                Id = 1, AcademicYearId = 1, StudentId = 3, OwnerId = 1, Delta = 10,
                IdempotencyKey = TestConstants.ExistingIdempotencyKey,
                CreatedAt = new DateTime(2025, 10, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = 2, AcademicYearId = 1, StudentId = 4, OwnerId = 1, Delta = 10,
                CreatedAt = new DateTime(2025, 10, 2, 10, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = 3, AcademicYearId = 1, StudentId = 5, OwnerId = 2, Delta = 50,
                CreatedAt = new DateTime(2025, 10, 3, 10, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = 4, AcademicYearId = 1, StudentId = 6, OwnerId = 2, Delta = 6,
                CreatedAt = new DateTime(2025, 10, 4, 10, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = 5, AcademicYearId = 1, StudentId = 1, OwnerId = 1, Delta = 5,
                Description = "Olympiad",
                CreatedAt = new DateTime(2025, 10, 5, 10, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = 6, AcademicYearId = 1, StudentId = 2, OwnerId = 1, Delta = 3,
                CreatedAt = new DateTime(2025, 10, 6, 10, 0, 0, DateTimeKind.Utc)
            }
        }.AsQueryable();
    }
}