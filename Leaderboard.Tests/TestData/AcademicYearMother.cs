using Leaderboard.Domain.Entities;
using Leaderboard.Tests.Constants;

namespace Leaderboard.Tests.TestData;

internal static class AcademicYearMother
{
    public static IQueryable<AcademicYear> GetAcademicYears()
    {
        return new AcademicYear[]
        {
            new()
            {
                Id = 1,
                Title = TestConstants.CurrentAcademicYearTitle,
                StartedAt = new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        }.AsQueryable();
    }
}