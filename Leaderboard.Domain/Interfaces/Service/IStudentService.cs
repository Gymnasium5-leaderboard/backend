using Leaderboard.Domain.Dtos.Student;
using Leaderboard.Domain.Results;

namespace Leaderboard.Domain.Interfaces.Service;

public interface IStudentService
{
    /// <summary>
    ///     Active students ordered by last and first name, optionally of one class.
    /// </summary>
    Task<CollectionResult<StudentDto>> GetAllAsync(long? classId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     An active student by id.
    /// </summary>
    Task<BaseResult<StudentDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<BaseResult<StudentDto>> CreateAsync(CreateStudentDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds all students in one save: if any of them is invalid, none is added.
    /// </summary>
    Task<CollectionResult<StudentDto>> CreateManyAsync(IReadOnlyCollection<CreateStudentDto> dtos,
        CancellationToken cancellationToken = default);

    Task<BaseResult<StudentDto>> UpdateAsync(long id, UpdateStudentDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Moves a student to another active class. The student keeps their scores.
    /// </summary>
    Task<BaseResult<StudentDto>> TransferAsync(long id, TransferStudentDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Marks a student as left the school. Their score history is kept.
    /// </summary>
    Task<BaseResult> DeactivateAsync(long id, CancellationToken cancellationToken = default);
}
