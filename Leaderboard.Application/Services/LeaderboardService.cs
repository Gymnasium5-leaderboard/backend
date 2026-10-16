using Leaderboard.Application.Enums;
using Leaderboard.Application.Extensions;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Category;
using Leaderboard.Domain.Dtos.Leaderboard;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Repository;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;
using Leaderboard.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Application.Services;

public class LeaderboardService(
    IBaseRepository<Student> studentRepository,
    IBaseRepository<SchoolClass> classRepository,
    IBaseRepository<ScoreTransaction> scoreTransactionRepository,
    IBaseRepository<AcademicYear> academicYearRepository,
    ICategoryService categoryService) : ILeaderboardService
{
    public Task<CollectionResult<ClassLeaderboardEntryDto>> GetSchoolClassesAsync(long? categoryId,
        CancellationToken cancellationToken = default)
    {
        return GetClassScoresAsync(null, categoryId, cancellationToken);
    }

    public Task<CollectionResult<ClassLeaderboardEntryDto>> GetGradeClassesAsync(int grade, long? categoryId,
        CancellationToken cancellationToken = default)
    {
        if (grade is < EntityConstraints.MinGrade or > EntityConstraints.MaxGrade)
            return Task.FromResult(CollectionResult<ClassLeaderboardEntryDto>.Failure(
                string.Format(ErrorMessage.InvalidGrade, EntityConstraints.MinGrade, EntityConstraints.MaxGrade),
                (int)ErrorCodes.InvalidProperty));

        return GetClassScoresAsync(grade, categoryId, cancellationToken);
    }

    public async Task<CollectionResult<StudentLeaderboardEntryDto>> GetClassStudentsAsync(long classId,
        long? categoryId, CancellationToken cancellationToken = default)
    {
        if (!await classRepository.GetAll().AnyAsync(x => x.Id == classId && x.IsActive, cancellationToken))
            return CollectionResult<StudentLeaderboardEntryDto>.Failure(ErrorMessage.ClassNotFound,
                (int)ErrorCodes.ClassNotFound);

        var yearId = await academicYearRepository.GetAll().GetCurrentIdAsync(cancellationToken);
        if (yearId == null)
            return CollectionResult<StudentLeaderboardEntryDto>.Failure(ErrorMessage.CurrentAcademicYearNotFound,
                (int)ErrorCodes.CurrentAcademicYearNotFound);

        var categories = await GetActiveCategoriesAsync(cancellationToken);
        if (categoryId != null && categories.All(x => x.Id != categoryId))
            return CollectionResult<StudentLeaderboardEntryDto>.Failure(ErrorMessage.CategoryNotFound,
                (int)ErrorCodes.CategoryNotFound);

        var students = await GetStudentScoresAsync(yearId.Value, classId, categoryId, cancellationToken);
        var categoryScores = await GetStudentCategoryScoresAsync(yearId.Value,
            students.Select(x => x.StudentId).ToArray(), cancellationToken);

        var entries = students
            .Select((x, i) => new StudentLeaderboardEntryDto(i + 1, x.StudentId, x.FirstName, x.LastName, x.Score,
                categories.Select(category => new StudentCategoryScoreDto(category.Id, category.Name,
                        categoryScores.GetValueOrDefault((x.StudentId, category.Id))))
                    .ToArray()))
            .ToArray();

        return CollectionResult<StudentLeaderboardEntryDto>.Success(entries);
    }

    public async Task<BaseResult<StudentRankDto>> GetStudentRankAsync(long studentId, long? categoryId,
        CancellationToken cancellationToken = default)
    {
        var student = await studentRepository.GetAll().AsNoTracking()
            .Include(x => x.Class)
            .FirstOrDefaultAsync(x => x.Id == studentId && x.IsActive && x.Class.IsActive, cancellationToken);
        if (student == null)
            return BaseResult<StudentRankDto>.Failure(ErrorMessage.StudentNotFound, (int)ErrorCodes.StudentNotFound);

        var yearId = await academicYearRepository.GetAll().GetCurrentIdAsync(cancellationToken);
        if (yearId == null)
            return BaseResult<StudentRankDto>.Failure(ErrorMessage.CurrentAcademicYearNotFound,
                (int)ErrorCodes.CurrentAcademicYearNotFound);

        var categories = await GetActiveCategoriesAsync(cancellationToken);
        if (categoryId != null && categories.All(x => x.Id != categoryId))
            return BaseResult<StudentRankDto>.Failure(ErrorMessage.CategoryNotFound,
                (int)ErrorCodes.CategoryNotFound);

        // The rank is among the classmates, so the whole class is ranked
        var classmates = await GetStudentScoresAsync(yearId.Value, student.ClassId, categoryId, cancellationToken);
        var index = Array.FindIndex(classmates, x => x.StudentId == studentId);
        var rank = index + 1;

        var categoryScores = await GetStudentCategoryScoresAsync(yearId.Value, [studentId], cancellationToken);

        var dto = new StudentRankDto(rank, student.Id, student.FirstName, student.LastName, classmates[index].Score,
            student.ClassId, student.Class.DisplayName,
            categories.Select(x => new StudentCategoryScoreDto(x.Id, x.Name,
                categoryScores.GetValueOrDefault((studentId, x.Id)))).ToArray());

        return BaseResult<StudentRankDto>.Success(dto);
    }

    private async Task<CollectionResult<ClassLeaderboardEntryDto>> GetClassScoresAsync(int? grade, long? categoryId,
        CancellationToken cancellationToken)
    {
        var yearId = await academicYearRepository.GetAll().GetCurrentIdAsync(cancellationToken);
        if (yearId == null)
            return CollectionResult<ClassLeaderboardEntryDto>.Failure(ErrorMessage.CurrentAcademicYearNotFound,
                (int)ErrorCodes.CurrentAcademicYearNotFound);

        var categories = await GetActiveCategoriesAsync(cancellationToken);
        if (categoryId != null && categories.All(x => x.Id != categoryId))
            return CollectionResult<ClassLeaderboardEntryDto>.Failure(ErrorMessage.CategoryNotFound,
                (int)ErrorCodes.CategoryNotFound);

        var transactions = GetTransactions(yearId.Value, categoryId)
            .Where(t => t.Student.IsActive);

        var classes = await classRepository.GetAll().AsNoTracking()
            .Where(c => c.IsActive && c.Students.Any(s => s.IsActive))
            .Where(c => grade == null || c.Grade == grade)
            .Select(c => new
            {
                Class = c,
                StudentCount = c.Students.Count(s => s.IsActive),
                Score = (double)transactions.Where(t => t.Student.ClassId == c.Id).Sum(t => t.Delta) /
                        c.Students.Count(s => s.IsActive),
                ReachedAt = transactions.Where(t => t.Student.ClassId == c.Id).Max(t => (DateTime?)t.CreatedAt)
            })
            .OrderByDescending(c => c.Score)
            .ThenBy(c => c.ReachedAt == null)
            .ThenBy(c => c.ReachedAt)
            .ThenBy(c => c.Class.Grade)
            .ThenBy(c => c.Class.Letter)
            .ToArrayAsync(cancellationToken);

        // Sums of every category, whatever the filter, divided by the student count below
        var classIds = classes.Select(c => c.Class.Id).ToArray();
        var categorySums = await GetTransactions(yearId.Value, null)
            .Where(t => t.Student.IsActive && classIds.Contains(t.Student.ClassId))
            .GroupBy(t => new { t.Student.ClassId, t.CategoryId })
            .Select(g => new { g.Key.ClassId, g.Key.CategoryId, Sum = g.Sum(t => t.Delta) })
            .ToDictionaryAsync(x => (x.ClassId, x.CategoryId), x => x.Sum, cancellationToken);

        var entries = classes.Select((c, i) =>
                new ClassLeaderboardEntryDto(i + 1, c.Class.Id, c.Class.DisplayName,
                    Math.Round(c.Score, 2, MidpointRounding.AwayFromZero),
                    categories.Select(x =>
                        new ClassCategoryScoreDto(x.Id, x.Name,
                            Math.Round((double)categorySums.GetValueOrDefault((c.Class.Id, x.Id)) / c.StudentCount, 2,
                                MidpointRounding.AwayFromZero))).ToArray()))
            .ToArray();

        return CollectionResult<ClassLeaderboardEntryDto>.Success(entries);
    }

    private async Task<StudentScore[]> GetStudentScoresAsync(long yearId, long classId, long? categoryId,
        CancellationToken cancellationToken)
    {
        var transactions = GetTransactions(yearId, categoryId);

        var students = await studentRepository.GetAll().AsNoTracking()
            .Where(x => x.IsActive && x.Class.IsActive)
            .Where(x => x.ClassId == classId)
            .Select(s => new
            {
                s.Id,
                s.FirstName,
                s.LastName,
                Score = transactions.Where(t => t.StudentId == s.Id).Sum(t => t.Delta),
                ReachedAt = transactions.Where(t => t.StudentId == s.Id).Max(t => (DateTime?)t.CreatedAt)
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.ReachedAt == null)
            .ThenBy(x => x.ReachedAt)
            .ThenBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ThenBy(x => x.Id)
            .ToArrayAsync(cancellationToken);

        return students.Select(x => new StudentScore(x.Id, x.FirstName, x.LastName, x.Score)).ToArray();
    }

    private Task<Dictionary<(long StudentId, long CategoryId), int>> GetStudentCategoryScoresAsync(long yearId,
        long[] studentIds, CancellationToken cancellationToken)
    {
        return GetTransactions(yearId, null)
            .Where(t => studentIds.Contains(t.StudentId))
            .GroupBy(t => new { t.StudentId, t.CategoryId })
            .Select(g => new { g.Key.StudentId, g.Key.CategoryId, Score = g.Sum(t => t.Delta) })
            .ToDictionaryAsync(x => (x.StudentId, x.CategoryId), x => x.Score, cancellationToken);
    }

    private IQueryable<ScoreTransaction> GetTransactions(long yearId, long? categoryId) => scoreTransactionRepository
        .GetAll().Where(t =>
            t.AcademicYearId == yearId && !t.Category.IsArchived && (categoryId == null || t.CategoryId == categoryId));


    private async Task<IReadOnlyCollection<CategoryDto>>
        GetActiveCategoriesAsync(CancellationToken cancellationToken) =>
        (await categoryService.GetAllAsync(false, cancellationToken)).Data ?? [];


    private sealed record StudentScore(
        long StudentId,
        string FirstName,
        string LastName,
        int Score);
}