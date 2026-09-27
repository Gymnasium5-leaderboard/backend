namespace Leaderboard.Domain.Settings;

public static class EntityConstraints
{
    public const int MinGrade = 4;
    public const int MaxGrade = 11;

    public const int NameMaxLength = 50;
    public const int LoginMinLength = 3;
    public const int LoginMaxLength = 50;
    public const int PasswordMinLength = 8;
    public const int AcademicYearTitleMaxLength = 20;
    public const int ScoreDescriptionMaxLength = 500;
}
