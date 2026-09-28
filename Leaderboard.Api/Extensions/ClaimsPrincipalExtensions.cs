using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Leaderboard.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    ///     Owner id from the "sub" claim, or <c>null</c> if the claim is missing or not a number.
    /// </summary>
    public static long? FindOwnerId(this ClaimsPrincipal user) =>
        long.TryParse(user.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;

    /// <summary>
    ///     Owner id of an authenticated request. <see cref="Middlewares.ClaimsValidationMiddleware" /> guarantees it.
    /// </summary>
    public static long GetOwnerId(this ClaimsPrincipal user) =>
        user.FindOwnerId() ?? throw new InvalidOperationException("The request has no owner id claim.");
}