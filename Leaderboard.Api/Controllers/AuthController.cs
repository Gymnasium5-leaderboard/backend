using Leaderboard.Api.Controllers.Base;
using Leaderboard.Api.Dtos;
using Leaderboard.Api.Extensions;
using Leaderboard.Application.Enums;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Auth;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Api.Controllers;

/// <summary>
///     Owner sign-in. The access token goes in the response body, the refresh token in an httpOnly cookie.
/// </summary>
public class AuthController(IAuthService authService) : BaseController
{
    /// <summary>
    ///     Signs in with login and password.
    /// </summary>
    /// <response code="200">Signed in</response>
    /// <response code="401">Invalid login or password</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BaseResult<AccessTokenDto>>> LoginAsync(LoginDto dto,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(dto, cancellationToken);
        return ToAccessTokenResult(result);
    }

    /// <summary>
    ///     Issues a new token pair for the refresh token cookie. The old refresh token stops working.
    /// </summary>
    /// <response code="200">Tokens refreshed</response>
    /// <response code="401">The refresh token is missing, invalid, revoked or expired</response>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BaseResult<AccessTokenDto>>> RefreshAsync(CancellationToken cancellationToken)
    {
        var refreshToken = Request.GetRefreshToken();
        var result = refreshToken == null
            ? BaseResult<TokenDto>.Failure(ErrorMessage.InvalidRefreshToken, (int)ErrorCodes.InvalidRefreshToken)
            : await authService.RefreshAsync(refreshToken, cancellationToken);

        return ToAccessTokenResult(result);
    }

    /// <summary>
    ///     Signs out: revokes the refresh token and deletes its cookie.
    /// </summary>
    /// <response code="204">Signed out</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<BaseResult>> LogoutAsync(CancellationToken cancellationToken)
    {
        var refreshToken = Request.GetRefreshToken();
        if (refreshToken != null) await authService.LogoutAsync(refreshToken, cancellationToken);

        Response.DeleteRefreshToken();
        return BaseResult.Success().ToActionResult();
    }

    private ActionResult<BaseResult<AccessTokenDto>> ToAccessTokenResult(BaseResult<TokenDto> result)
    {
        if (!result.IsSuccess)
        {
            Response.DeleteRefreshToken();
            return BaseResult<AccessTokenDto>.Failure(result.ErrorMessage!, result.ErrorCode).ToActionResult();
        }

        Response.SetRefreshToken(result.Data.RefreshToken, result.Data.RefreshTokenExpiresAt);
        return BaseResult<AccessTokenDto>
            .Success(new AccessTokenDto(result.Data.AccessToken, result.Data.AccessTokenExpiresAt))
            .ToActionResult();
    }
}