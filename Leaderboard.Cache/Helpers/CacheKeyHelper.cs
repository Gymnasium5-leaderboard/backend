namespace Leaderboard.Cache.Helpers;

public static class CacheKeyHelper
{
    private const string ClassPattern = "class:{0}";
    private const string GradeClassesPattern = "grade:{0}:classes";
    private const string CurrentAcademicYearKey = "academicYear:current";
    private const string AllGradesPlaceholder = "all";
    private const string AnyPlaceholder = "*";

    public static string GetClassKey(long id)
    {
        return string.Format(ClassPattern, id);
    }

    public static string GetGradeClassesKey(int? grade)
    {
        return string.Format(GradeClassesPattern, grade?.ToString() ?? AllGradesPlaceholder);
    }

    public static string GetCurrentAcademicYearKey()
    {
        return CurrentAcademicYearKey;
    }

    public static string[] GetAllClassesKeyPatterns()
    {
        return [string.Format(ClassPattern, AnyPlaceholder), string.Format(GradeClassesPattern, AnyPlaceholder)];
    }
}