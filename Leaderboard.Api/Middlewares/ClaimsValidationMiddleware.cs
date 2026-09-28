using Leaderboard.Api.Extensions;
using Leaderboard.Application.Enums;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;

namespace Leaderboard.Api.Middlewares;

/// <summary>
///     Rejects a valid JWT whose owner id is malformed or no longer exists.
/// </summary>
public class ClaimsValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated != true)
        {
            await next(httpContext);
            return;
        }

        var ownerId = httpContext.User.FindOwnerId();
        if (ownerId == null)
        {
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await httpContext.Response.WriteAsJsonAsync(
                BaseResult.Failure(ErrorMessage.InvalidClaims, (int)ErrorCodes.InvalidClaims), CancellationToken.None);
            return;
        }

        await next(httpContext);
    }
}