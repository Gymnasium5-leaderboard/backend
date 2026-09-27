namespace Leaderboard.Domain.Settings;

/// <summary>
///     System settings for scoring, bound from the "BusinessRules" configuration section.
/// </summary>
public class BusinessRules
{
    /// <summary>
    ///     Max absolute value of a single score change.
    /// </summary>
    public int MaxScoreDelta { get; set; } = 100;

    /// <summary>
    ///     Whether a student's score may go below zero.
    /// </summary>
    public bool AllowNegativeScore { get; set; }
}
