using AutoMapper;
using EntityFramework.Exceptions.Common;
using Leaderboard.Application.Enums;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.AcademicYear;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Repository;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;
using Leaderboard.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Application.Services;

public class AcademicYearService(
    IUnitOfWork unitOfWork,
    IBaseRepository<SchoolClass> classRepository,
    IBaseRepository<AcademicYear> academicYearRepository,
    IMapper mapper) : IAcademicYearService, IAcademicYearInitializer
{
    private const int FirstMonthOfAcademicYear = 9;

    public async Task EnsureCurrentAsync(CancellationToken cancellationToken = default)
    {
        if (await academicYearRepository.GetAll().AnyAsync(cancellationToken)) return;

        // A school year starts on September 1: in October 2026 and in May 2027 it is "2026/2027"
        var now = DateTime.UtcNow;
        var startYear = now.Month >= FirstMonthOfAcademicYear ? now.Year : now.Year - 1;

        await academicYearRepository.CreateAsync(new AcademicYear
        {
            Title = $"{startYear}/{startYear + 1}",
            StartedAt = now
        }, cancellationToken);

        try
        {
            await academicYearRepository.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException)
        {
            // Another instance created it at the same time
        }
    }

    public async Task<BaseResult<AcademicYearDto>> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var year = await academicYearRepository.GetAll().AsNoTracking()
            .FirstOrDefaultAsync(x => x.FinishedAt == null, cancellationToken);
        if (year == null) return CurrentAcademicYearNotFound();

        return BaseResult<AcademicYearDto>.Success(mapper.Map<AcademicYearDto>(year));
    }

    public async Task<BaseResult<AcademicYearDto>> StartNewAsync(CancellationToken cancellationToken = default)
    {
        var current = await academicYearRepository.GetAll().AsNoTracking()
            .FirstOrDefaultAsync(x => x.FinishedAt == null, cancellationToken);
        if (current == null) return CurrentAcademicYearNotFound();

        // The new year is named by the calendar year it starts in: started in 2027 -> "2027/2028"
        var now = DateTime.UtcNow;
        var title = $"{now.Year}/{now.Year + 1}";

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        // The row lock serializes concurrent requests: the second one finds the year already closed
        var closed = await academicYearRepository.GetAll()
            .Where(x => x.Id == current.Id && x.FinishedAt == null && x.Title != title)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.FinishedAt, now), cancellationToken);
        if (closed == 0)
            return BaseResult<AcademicYearDto>.Failure(ErrorMessage.AcademicYearAlreadyChanged,
                (int)ErrorCodes.AcademicYearAlreadyChanged);

        // Graduation: students first, while their classes are still active.
        // ExecuteUpdate bypasses DateInterceptor, so LastModifiedAt is set here
        await unitOfWork.Students.GetAll()
            .Where(x => x.IsActive && x.Class.IsActive && x.Class.Grade == EntityConstraints.MaxGrade)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.IsActive, false)
                .SetProperty(x => x.LastModifiedAt, now), cancellationToken);
        await classRepository.GetAll()
            .Where(x => x.IsActive && x.Grade == EntityConstraints.MaxGrade)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false), cancellationToken);

        // From the top grade down, so 7A never meets an existing 8A in the unique index
        for (var grade = EntityConstraints.MaxGrade - 1; grade >= EntityConstraints.MinGrade; grade--)
            await classRepository.GetAll()
                // ReSharper disable once AccessToModifiedClosure
                // ExecuteUpdateAsync executes the query immediately, so the closure is safe here
                .Where(x => x.IsActive && x.Grade == grade)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Grade, grade + 1), cancellationToken);

        var year = new AcademicYear { Title = title, StartedAt = now };
        await academicYearRepository.CreateAsync(year, cancellationToken);
        await academicYearRepository.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return BaseResult<AcademicYearDto>.Success(mapper.Map<AcademicYearDto>(year));
    }

    private static BaseResult<AcademicYearDto> CurrentAcademicYearNotFound()
    {
        return BaseResult<AcademicYearDto>.Failure(ErrorMessage.CurrentAcademicYearNotFound,
            (int)ErrorCodes.CurrentAcademicYearNotFound);
    }
}