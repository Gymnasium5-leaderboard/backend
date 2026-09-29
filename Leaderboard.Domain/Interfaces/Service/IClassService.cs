using Leaderboard.Domain.Dtos.Class;
using Leaderboard.Domain.Results;

namespace Leaderboard.Domain.Interfaces.Service;

public interface IClassService
{
    /// <summary>
    ///     Active classes ordered by grade and letter, optionally of one grade.
    /// </summary>
    Task<CollectionResult<ClassDto>> GetAllAsync(int? grade, CancellationToken cancellationToken = default);

    /// <summary>
    ///     An active class by id.
    /// </summary>
    Task<BaseResult<ClassDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<BaseResult<ClassDto>> CreateAsync(CreateClassDto dto, CancellationToken cancellationToken = default);

    Task<BaseResult<ClassDto>> UpdateAsync(long id, UpdateClassDto dto, CancellationToken cancellationToken = default);
}
