namespace Leaderboard.Api.Extensions;

/// <summary>
///     The refresh token lives only in an httpOnly cookie, so page scripts cannot read it.
/// </summary>
public static class RefreshTokenCookieExtensions
{
    private const string CookieName = "refreshToken";

    public static void SetRefreshToken(this HttpResponse response, string refreshToken, DateTime expiresAt) =>
        response.Cookies.Append(CookieName, refreshToken, GetOptions(expiresAt));

    public static void DeleteRefreshToken(this HttpResponse response) =>
        response.Cookies.Delete(CookieName, GetOptions());

    public static string? GetRefreshToken(this HttpRequest request) => request.Cookies[CookieName];

    // Sent only to the API; SameSite=None because the frontend may be on another site
    private static CookieOptions GetOptions(DateTime? expiresAt = null)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/api",
            Expires = expiresAt
        };
    }
}