namespace Leaderboard.Cache.Helpers;

public static class CacheKeyHelper
{
    private const string ClassPattern = "class:{0}";
    private const string GradeClassesPattern = "grade:{0}:classes";
    private const string CurrentAcademicYearKey = "academicYear:current";
    private const string LeaderboardVersionKey = "leaderboard:version";
    private const string CategoriesKey = "categories";
    private const string SchoolClassesLeaderboardPattern = "leaderboard:{0}:classes:{1}";
    private const string GradeClassesLeaderboardPattern = "leaderboard:{0}:grade:{1}:classes:{2}";
    private const string ClassStudentsLeaderboardPattern = "leaderboard:{0}:class:{1}:students:{2}";
    private const string StudentRankPattern = "leaderboard:{0}:student:{1}:{2}";
    private const string LeaderboardChangedChannel = "leaderboard:changed";
    private const string AllPlaceholder = "all";
    private const string AnyPlaceholder = "*";

    public static string GetClassKey(long id)
    {
        return string.Format(ClassPattern, id);
    }

    public static string GetGradeClassesKey(int? grade)
    {
        return string.Format(GradeClassesPattern, grade?.ToString() ?? AllPlaceholder);
    }

    public static string GetCurrentAcademicYearKey()
    {
        return CurrentAcademicYearKey;
    }

    public static string[] GetAllClassesKeyPatterns()
    {
        return [string.Format(ClassPattern, AnyPlaceholder), string.Format(GradeClassesPattern, AnyPlaceholder)];
    }

    public static string GetCategoriesKey()
    {
        return CategoriesKey;
    }

    public static string GetLeaderboardVersionKey()
    {
        return LeaderboardVersionKey;
    }

    public static string GetSchoolClassesLeaderboardKey(long version, long? categoryId)
    {
        return string.Format(SchoolClassesLeaderboardPattern, version, GetCategoryPart(categoryId));
    }

    public static string GetGradeClassesLeaderboardKey(long version, int grade, long? categoryId)
    {
        return string.Format(GradeClassesLeaderboardPattern, version, grade, GetCategoryPart(categoryId));
    }

    public static string GetClassStudentsLeaderboardKey(long version, long classId, long? categoryId)
    {
        return string.Format(ClassStudentsLeaderboardPattern, version, classId, GetCategoryPart(categoryId));
    }

    public static string GetStudentRankKey(long version, long studentId, long? categoryId)
    {
        return string.Format(StudentRankPattern, version, studentId, GetCategoryPart(categoryId));
    }

    public static string GetLeaderboardChangedChannel()
    {
        return LeaderboardChangedChannel;
    }

    private static string GetCategoryPart(long? categoryId)
    {
        return categoryId?.ToString() ?? AllPlaceholder;
    }
}