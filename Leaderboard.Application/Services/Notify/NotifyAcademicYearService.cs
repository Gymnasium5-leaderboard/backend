using Leaderboard.Domain.Dtos.AcademicYear;
using Leaderboard.Domain.Interfaces.Notifier;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;

namespace Leaderboard.Application.Services.Notify;

public class NotifyAcademicYearService(ILeaderboardNotifier leaderboardNotifier, IAcademicYearService inner)
    : IAcademicYearService
{
    public Task<BaseResult<AcademicYearDto>> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        return inner.GetCurrentAsync(cancellationToken);
    }

    public async Task<BaseResult<AcademicYearDto>> StartNewAsync(CancellationToken cancellationToken = default)
    {
        var result = await inner.StartNewAsync(cancellationToken);
        if (result.IsSuccess) await leaderboardNotifier.NotifyChangedAsync();

        return result;
    }
}