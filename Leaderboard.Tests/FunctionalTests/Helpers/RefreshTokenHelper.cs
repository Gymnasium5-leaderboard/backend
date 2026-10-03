namespace Leaderboard.Tests.FunctionalTests.Helpers;

internal static class RefreshTokenHelper
{
    private const string CookieName = "refreshToken";

    public static Task<HttpResponseMessage> PostWithRefreshTokenAsync(this HttpClient httpClient, string url,
        string refreshToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Cookie", $"{CookieName}={refreshToken}");

        return httpClient.SendAsync(request);
    }

    /// <summary>
    ///     Returns the refresh token from the Set-Cookie header, or null when the cookie is missing or deleted.
    /// </summary>
    public static string? GetRefreshToken(this HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies)) return null;

        return cookies
            .Select(x => x.Split(';')[0])
            .Where(x => x.StartsWith($"{CookieName}="))
            .Select(x => x[(CookieName.Length + 1)..])
            .FirstOrDefault(x => x.Length > 0);
    }
}