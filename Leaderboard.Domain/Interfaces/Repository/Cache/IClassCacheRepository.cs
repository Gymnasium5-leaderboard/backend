using Leaderboard.Domain.Dtos.Class;

namespace Leaderboard.Domain.Interfaces.Repository.Cache;

/// <summary>
///     Classes are stored by id, and a grade list stores only the ids of its classes. Cache errors are logged, not
///     thrown: a failed read is a miss.
/// </summary>
public interface IClassCacheRepository
{
    /// <summary>
    ///     A class by id, or null on a cache miss.
    /// </summary>
    Task<ClassDto?> GetAsync(long id, CancellationToken cancellationToken = default);

    Task SetAsync(ClassDto schoolClass);

    /// <summary>
    ///     Classes of a grade, or of all grades if <paramref name="grade" /> is null, in the order they were cached.
    ///     Null on a cache miss, including when any class of the list has expired.
    /// </summary>
    Task<IReadOnlyCollection<ClassDto>?> GetByGradeAsync(int? grade, CancellationToken cancellationToken = default);

    Task SetByGradeAsync(int? grade, IReadOnlyCollection<ClassDto> classes);

    /// <summary>
    ///     Removes the class and the lists of all grades, because the class may have moved to another grade.
    /// </summary>
    Task RemoveAsync(long id);

    /// <summary>
    ///     Removes the lists of the given grades and the list of all classes.
    /// </summary>
    Task RemoveGradesAsync(IReadOnlyCollection<int> grades);

    /// <summary>
    ///     Removes all classes and grade lists.
    /// </summary>
    Task RemoveAllAsync();
}