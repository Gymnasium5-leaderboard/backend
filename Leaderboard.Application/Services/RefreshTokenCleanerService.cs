using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Repository;
using Leaderboard.Domain.Interfaces.Service;
using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Application.Services;

public class RefreshTokenCleanerService(IBaseRepository<RefreshToken> refreshTokenRepository)
    : IRefreshTokenCleanerService
{
    // Revoked tokens are removed too, once they expire
    public Task<int> RemoveExpiredAsync(CancellationToken cancellationToken = default)
    {
        return refreshTokenRepository.GetAll()
            .Where(x => x.ExpiresAt <= DateTime.UtcNow)
            .ExecuteDeleteAsync(cancellationToken);
    }
}