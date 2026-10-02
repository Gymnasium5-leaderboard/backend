using Leaderboard.Application.Enums;
using Leaderboard.Application.Extensions;
using Leaderboard.Application.Resources;
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
    IBaseRepository<AcademicYear> academicYearRepository) : ILeaderboardService
{
    public Task<CollectionResult<ClassLeaderboardEntryDto>> GetSchoolClassesAsync(
        CancellationToken cancellationToken = default)
    {
        return GetClassScoresAsync(null, cancellationToken);
    }


    public Task<CollectionResult<ClassLeaderboardEntryDto>> GetGradeClassesAsync(int grade,
        CancellationToken cancellationToken = default)
    {
        if (grade is < EntityConstraints.MinGrade or > EntityConstraints.MaxGrade)
            return Task.FromResult(CollectionResult<ClassLeaderboardEntryDto>.Failure(
                string.Format(ErrorMessage.InvalidGrade, EntityConstraints.MinGrade, EntityConstraints.MaxGrade),
                (int)ErrorCodes.InvalidProperty));

        return GetClassScoresAsync(grade, cancellationToken);
    }

    public async Task<CollectionResult<StudentLeaderboardEntryDto>> GetClassStudentsAsync(long classId,
        CancellationToken cancellationToken = default)
    {
        if (!await classRepository.GetAll().AnyAsync(x => x.Id == classId && x.IsActive, cancellationToken))
            return CollectionResult<StudentLeaderboardEntryDto>.Failure(ErrorMessage.ClassNotFound,
                (int)ErrorCodes.ClassNotFound);

        var yearId = await academicYearRepository.GetAll().GetCurrentIdAsync(cancellationToken);
        if (yearId == null)
            return CollectionResult<StudentLeaderboardEntryDto>.Failure(ErrorMessage.CurrentAcademicYearNotFound,
                (int)ErrorCodes.CurrentAcademicYearNotFound);

        var students = await GetStudentScoresAsync(yearId.Value, classId, cancellationToken);

        var entries = students
            .Select((x, i) => new StudentLeaderboardEntryDto(i + 1, x.StudentId, x.FirstName, x.LastName, x.Score))
            .ToArray();

        return CollectionResult<StudentLeaderboardEntryDto>.Success(entries);
    }

    public async Task<BaseResult<StudentPlaceDto>> GetStudentPlaceAsync(long studentId,
        CancellationToken cancellationToken = default)
    {
        var student = await studentRepository.GetAll().AsNoTracking()
            .Include(x => x.Class)
            .FirstOrDefaultAsync(x => x.Id == studentId && x.IsActive && x.Class.IsActive, cancellationToken);
        if (student == null)
            return BaseResult<StudentPlaceDto>.Failure(ErrorMessage.StudentNotFound, (int)ErrorCodes.StudentNotFound);

        var yearId = await academicYearRepository.GetAll().GetCurrentIdAsync(cancellationToken);
        if (yearId == null)
            return BaseResult<StudentPlaceDto>.Failure(ErrorMessage.CurrentAcademicYearNotFound,
                (int)ErrorCodes.CurrentAcademicYearNotFound);

        // The place is among the classmates, so the whole class is ranked
        var classmates = await GetStudentScoresAsync(yearId.Value, student.ClassId, cancellationToken);
        var index = Array.FindIndex(classmates, x => x.StudentId == studentId);
        var place = index + 1;

        var dto = new StudentPlaceDto(place, student.Id, student.FirstName, student.LastName, classmates[index].Score,
            student.ClassId, student.Class.DisplayName);

        return BaseResult<StudentPlaceDto>.Success(dto);
    }

    private async Task<CollectionResult<ClassLeaderboardEntryDto>> GetClassScoresAsync(int? grade,
        CancellationToken cancellationToken)
    {
        var yearId = await academicYearRepository.GetAll().GetCurrentIdAsync(cancellationToken);
        if (yearId == null)
            return CollectionResult<ClassLeaderboardEntryDto>.Failure(ErrorMessage.CurrentAcademicYearNotFound,
                (int)ErrorCodes.CurrentAcademicYearNotFound);

        var transactions = scoreTransactionRepository.GetAll()
            .Where(t => t.AcademicYearId == yearId && t.Student.IsActive);

        var classes = await classRepository.GetAll().AsNoTracking()
            .Where(c => c.IsActive && c.Students.Any(s => s.IsActive))
            .Where(c => grade == null || c.Grade == grade)
            .Select(c => new
            {
                Class = c,
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

        var entries = classes
            .Select((c, i) =>
                new ClassLeaderboardEntryDto(i + 1, c.Class.Id, c.Class.DisplayName,
                    Math.Round(c.Score, 2, MidpointRounding.AwayFromZero)))
            .ToArray();

        return CollectionResult<ClassLeaderboardEntryDto>.Success(entries);
    }

    private Task<StudentScore[]> GetStudentScoresAsync(long yearId, long classId, CancellationToken cancellationToken)
    {
        var transactions = scoreTransactionRepository.GetAll().Where(t => t.AcademicYearId == yearId);
        return studentRepository.GetAll().AsNoTracking()
            .Where(x => x.IsActive && x.Class.IsActive)
            .Where(x => x.ClassId == classId)
            .Select(s => new StudentScore(
                s.Id,
                s.FirstName,
                s.LastName,
                transactions.Where(t => t.StudentId == s.Id).Sum(t => t.Delta),
                transactions.Where(t => t.StudentId == s.Id).Max(t => (DateTime?)t.CreatedAt)
            ))
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.ReachedAt == null)
            .ThenBy(x => x.ReachedAt)
            .ThenBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ThenBy(x => x.StudentId)
            .ToArrayAsync(cancellationToken);
    }

    private sealed record StudentScore(
        long StudentId,
        string FirstName,
        string LastName,
        int Score,
        DateTime? ReachedAt);
}