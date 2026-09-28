using Leaderboard.Domain.Dtos.Auth;
using Leaderboard.Domain.Results;

namespace Leaderboard.Domain.Interfaces.Service;

public interface IAuthService
{
    Task<BaseResult<TokenDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Exchanges a refresh token for a new token pair. The old refresh token is revoked (rotation).
    /// </summary>
    Task<BaseResult<TokenDto>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Revokes the refresh token. Succeeds even if the token is unknown.
    /// </summary>
    Task<BaseResult> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Changes the owner's password and revokes all their refresh tokens.
    /// </summary>
    Task<BaseResult> ChangePasswordAsync(long ownerId, ChangePasswordDto dto,
        CancellationToken cancellationToken = default);
}