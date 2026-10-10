using Leaderboard.Domain.Dtos.Class;
using Leaderboard.Domain.Interfaces.Notifier;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;

namespace Leaderboard.Application.Services.Notify;

/// <summary>
///     A new class has no students yet, so only an update changes the leaderboards.
/// </summary>
public class NotifyClassService(ILeaderboardNotifier leaderboardNotifier, IClassService inner) : IClassService
{
    public Task<CollectionResult<ClassDto>> GetAllAsync(int? grade, CancellationToken cancellationToken = default)
    {
        return inner.GetAllAsync(grade, cancellationToken);
    }

    public Task<BaseResult<ClassDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return inner.GetByIdAsync(id, cancellationToken);
    }

    public Task<BaseResult<ClassDto>> CreateAsync(CreateClassDto dto, CancellationToken cancellationToken = default)
    {
        return inner.CreateAsync(dto, cancellationToken);
    }

    public async Task<BaseResult<ClassDto>> UpdateAsync(long id, UpdateClassDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await inner.UpdateAsync(id, dto, cancellationToken);
        if (result.IsSuccess) await leaderboardNotifier.NotifyChangedAsync();

        return result;
    }
}