namespace Leaderboard.Tests.Constants;

internal static class TestConstants
{
    public const string TestPassword = "TestPassword";
    public const string WrongPassword = "WrongPassword";
    public const string NewPassword = "NewTestPassword";

    public const string RefreshToken = "TestRefreshToken";
    public const string RevokedRefreshToken = "RevokedTestRefreshToken";
    public const string ExpiredRefreshToken = "ExpiredTestRefreshToken";
    public const string WrongRefreshToken = "WrongTestRefreshToken";

    public const string CurrentAcademicYearTitle = "2025/2026";

    public static readonly Guid ExistingIdempotencyKey = Guid.Parse("8f2c1a52-6d0e-4b8a-9c3f-1e7a5b4d2c10");
}