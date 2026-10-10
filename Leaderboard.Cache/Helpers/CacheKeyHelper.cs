namespace Leaderboard.Cache.Helpers;

public static class CacheKeyHelper
{
    private const string ClassPattern = "class:{0}";
    private const string GradeClassesPattern = "grade:{0}:classes";
    private const string CurrentAcademicYearKey = "academicYear:current";
    private const string LeaderboardVersionKey = "leaderboard:version";
    private const string SchoolClassesLeaderboardPattern = "leaderboard:{0}:classes";
    private const string GradeClassesLeaderboardPattern = "leaderboard:{0}:grade:{1}:classes";
    private const string ClassStudentsLeaderboardPattern = "leaderboard:{0}:class:{1}:students";
    private const string StudentRankPattern = "leaderboard:{0}:student:{1}";
    private const string LeaderboardChangedChannel = "leaderboard:changed";
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

    public static string GetLeaderboardVersionKey()
    {
        return LeaderboardVersionKey;
    }

    public static string GetSchoolClassesLeaderboardKey(long version)
    {
        return string.Format(SchoolClassesLeaderboardPattern, version);
    }

    public static string GetGradeClassesLeaderboardKey(long version, int grade)
    {
        return string.Format(GradeClassesLeaderboardPattern, version, grade);
    }

    public static string GetClassStudentsLeaderboardKey(long version, long classId)
    {
        return string.Format(ClassStudentsLeaderboardPattern, version, classId);
    }

    public static string GetStudentRankKey(long version, long studentId)
    {
        return string.Format(StudentRankPattern, version, studentId);
    }

    public static string GetLeaderboardChangedChannel()
    {
        return LeaderboardChangedChannel;
    }
}