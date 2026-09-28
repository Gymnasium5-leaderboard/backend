using Leaderboard.Domain.Entities;

namespace Leaderboard.Domain.Interfaces.Provider;

public interface ITokenProvider
{
    /// <summary>
    ///     Creates a signed JWT access token with the owner id in the "sub" claim.
    /// </summary>
    (string Token, DateTime ExpiresAt) CreateAccessToken(LeaderboardOwner owner);
}
