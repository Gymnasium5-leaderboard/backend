using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Repository;

namespace Leaderboard.Application.Extensions;

internal static class ScoreExtensions
{
    /// <summary>
    ///     Transactions of the year in active categories, of one category if it is given.
    ///     A null year has no transactions.
    /// </summary>
    public static IQueryable<ScoreTransaction> GetTransactions(this IBaseRepository<ScoreTransaction> repository,
        long? yearId, long? categoryId) => repository.GetAll().Where(t =>
        t.AcademicYearId == yearId && !t.Category.IsArchived && (categoryId == null || t.CategoryId == categoryId));


    /// <summary>
    ///     Score of a class is the sum of its active students' transactions divided by their count.
    /// </summary>
    public static IQueryable<ClassScore> SelectScores(this IQueryable<SchoolClass> classes,
        IQueryable<ScoreTransaction> transactions)
    {
        var active = transactions.Where(t => t.Student.IsActive);

        return classes.Select(c => new ClassScore
        {
            Class = c,
            StudentCount = c.Students.Count(s => s.IsActive),
            // A class without students would divide by zero
            Score = c.Students.Any(s => s.IsActive)
                ? (double)active.Where(t => t.Student.ClassId == c.Id).Sum(t => t.Delta) /
                  c.Students.Count(s => s.IsActive)
                : 0,
            ReachedAt = active.Where(t => t.Student.ClassId == c.Id).Max(t => (DateTime?)t.CreatedAt)
        });
    }
}

internal sealed class ClassScore
{
    public required SchoolClass Class { get; init; }
    public required int StudentCount { get; init; }
    public required double Score { get; init; }
    public required DateTime? ReachedAt { get; init; }
}