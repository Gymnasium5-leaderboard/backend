using AutoMapper;
using EntityFramework.Exceptions.Common;
using FluentValidation;
using Leaderboard.Application.Enums;
using Leaderboard.Application.Extensions;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Score;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Repository;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Interfaces.Validation;
using Leaderboard.Domain.Results;
using Leaderboard.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NGuid;

namespace Leaderboard.Application.Services;

public class ScoreService(
    IUnitOfWork unitOfWork,
    IBaseRepository<SchoolClass> classRepository,
    IBaseRepository<AcademicYear> academicYearRepository,
    IValidator<IValidatableScore> scoreValidator,
    IValidator<ChangeScoreBatchDto> batchValidator,
    IValidator<ScoreHistoryQueryDto> historyValidator,
    IOptions<BusinessRules> businessRules,
    IMapper mapper) : IScoreService
{
    private readonly BusinessRules _businessRules = businessRules.Value;

    public async Task<BaseResult<ScoreChangedDto>> ChangeScoreAsync(long ownerId, ChangeScoreDto dto,
        Guid? idempotencyKey, CancellationToken cancellationToken = default)
    {
        var (isValid, errorMessage) = await scoreValidator.ValidateWithMessageAsync(dto, cancellationToken);
        if (!isValid) return BaseResult<ScoreChangedDto>.Failure(errorMessage, (int)ErrorCodes.InvalidProperty);

        var result = await ChangeScoresAsync(ownerId, [dto.StudentId], dto.Delta, dto.Description,
            _ => idempotencyKey, cancellationToken);
        if (!result.IsSuccess) return BaseResult<ScoreChangedDto>.Failure(result.ErrorMessage!, result.ErrorCode);

        return BaseResult<ScoreChangedDto>.Success(result.Data.Single());
    }

    public async Task<CollectionResult<ScoreChangedDto>> ChangeScoreBatchAsync(long ownerId, ChangeScoreBatchDto dto,
        Guid? idempotencyKey, CancellationToken cancellationToken = default)
    {
        var (isValid, errorMessage) = await batchValidator.ValidateWithMessageAsync(dto, cancellationToken);
        if (!isValid)
            return CollectionResult<ScoreChangedDto>.Failure(errorMessage, (int)ErrorCodes.InvalidProperty);

        long[] studentIds;
        if (dto.ClassId != null)
        {
            if (!await classRepository.GetAll().AnyAsync(x => x.Id == dto.ClassId && x.IsActive, cancellationToken))
                return CollectionResult<ScoreChangedDto>.Failure(ErrorMessage.ClassNotFound,
                    (int)ErrorCodes.ClassNotFound);

            studentIds = await unitOfWork.Students.GetAll()
                .Where(x => x.ClassId == dto.ClassId && x.IsActive)
                .Select(x => x.Id)
                .ToArrayAsync(cancellationToken);
            if (studentIds.Length == 0)
                return CollectionResult<ScoreChangedDto>.Failure(ErrorMessage.StudentListEmpty,
                    (int)ErrorCodes.InvalidProperty);
        }
        else
        {
            studentIds = dto.StudentIds!.Distinct().ToArray();
        }

        // Each row gets its own key derived from the batch key, so a repeated batch hits the UNIQUE index
        return await ChangeScoresAsync(ownerId, studentIds, dto.Delta, dto.Description,
            studentId => idempotencyKey == null
                ? null
                : GuidHelpers.CreateFromName(idempotencyKey.Value, studentId.ToString()),
            cancellationToken);
    }

    public async Task<PagedResult<ScoreTransactionDto>> GetHistoryAsync(ScoreHistoryQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var (isValid, errorMessage) = await historyValidator.ValidateWithMessageAsync(query, cancellationToken);
        if (!isValid) return PagedResult<ScoreTransactionDto>.Failure(errorMessage, (int)ErrorCodes.InvalidProperty);

        var yearId = await GetCurrentAcademicYearIdAsync(cancellationToken);
        if (yearId == null)
            return PagedResult<ScoreTransactionDto>.Failure(ErrorMessage.CurrentAcademicYearNotFound,
                (int)ErrorCodes.CurrentAcademicYearNotFound);

        // Students who left the school are included: their history is kept
        var transactions = unitOfWork.ScoreTransactions.GetAll().AsNoTracking()
            .Where(x => x.AcademicYearId == yearId
                        && (query.StudentId == null || x.StudentId == query.StudentId)
                        && (query.ClassId == null || x.Student.ClassId == query.ClassId));

        var totalCount = await transactions.CountAsync(cancellationToken);
        var page = await transactions
            .OrderByDescending(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToArrayAsync(cancellationToken);

        return PagedResult<ScoreTransactionDto>.Success(mapper.Map<ScoreTransactionDto[]>(page), totalCount, query.Page,
            query.PageSize);
    }

    /// <summary>
    ///     Adds one transaction per student in one database transaction. Changes of the same student are serialized
    ///     by a lock on the student id, so concurrent write-offs cannot both pass the negative score check.
    /// </summary>
    private async Task<CollectionResult<ScoreChangedDto>> ChangeScoresAsync(long ownerId,
        IReadOnlyCollection<long> studentIds, int delta, string? description, Func<long, Guid?> getIdempotencyKey,
        CancellationToken cancellationToken)
    {
        var yearId = await GetCurrentAcademicYearIdAsync(cancellationToken);
        if (yearId == null)
            return CollectionResult<ScoreChangedDto>.Failure(ErrorMessage.CurrentAcademicYearNotFound,
                (int)ErrorCodes.CurrentAcademicYearNotFound);

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        await unitOfWork.AcquireLockAsync(studentIds, cancellationToken);

        var keys = studentIds.Select(getIdempotencyKey).Where(x => x != null).ToArray();
        if (keys.Length > 0)
        {
            var existing = await GetExistingAsync(yearId.Value, keys, cancellationToken);
            if (existing.Length > 0) return CollectionResult<ScoreChangedDto>.Success(existing);
        }

        var activeIds = await unitOfWork.Students.GetAll()
            .Where(x => studentIds.Contains(x.Id) && x.IsActive)
            .Select(x => x.Id)
            .ToArrayAsync(cancellationToken);
        var notFoundIds = studentIds.Except(activeIds).Order().ToArray();
        if (notFoundIds.Length > 0) return StudentsNotFound(notFoundIds);

        var orderedIds = studentIds.Order().ToArray();

        var scores = await GetScoresAsync(yearId.Value, orderedIds, cancellationToken);

        if (!_businessRules.AllowNegativeScore)
        {
            var failedIds = orderedIds.Where(id => scores.GetValueOrDefault(id) + delta < 0).ToArray();
            if (failedIds.Length > 0) return ScoreWouldBeNegative(failedIds);
        }

        var now = DateTime.UtcNow;
        var newTransactions = orderedIds.Select(studentId => new ScoreTransaction
        {
            AcademicYearId = yearId.Value,
            StudentId = studentId,
            OwnerId = ownerId,
            Delta = delta,
            Description = description,
            IdempotencyKey = getIdempotencyKey(studentId),
            CreatedAt = now
        }).ToArray();
        await unitOfWork.ScoreTransactions.CreateRangeAsync(newTransactions, cancellationToken);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException e)
            when (e.ConstraintProperties.Contains(nameof(ScoreTransaction.IdempotencyKey)))
        {
            // The key was already used by a request for other students: return its result if it exists
            await transaction.RollbackAsync(cancellationToken);
            var existing = await GetExistingAsync(yearId.Value, keys, cancellationToken);
            if (existing.Length == 0) throw;

            return CollectionResult<ScoreChangedDto>.Success(existing);
        }

        await transaction.CommitAsync(cancellationToken);

        return CollectionResult<ScoreChangedDto>.Success(newTransactions
            .Select(x => new ScoreChangedDto(x.Id, x.StudentId, x.Delta,
                scores.GetValueOrDefault(x.StudentId) + x.Delta, x.CreatedAt))
            .ToArray());
    }

    private async Task<ScoreChangedDto[]> GetExistingAsync(long yearId, Guid?[] keys,
        CancellationToken cancellationToken)
    {
        var existing = await unitOfWork.ScoreTransactions.GetAll().AsNoTracking()
            .Where(x => keys.Contains(x.IdempotencyKey))
            .OrderBy(x => x.StudentId)
            .ToArrayAsync(cancellationToken);

        var scores = await GetScoresAsync(yearId, existing.Select(x => x.StudentId).ToArray(), cancellationToken);

        return existing
            .Select(x => new ScoreChangedDto(x.Id, x.StudentId, x.Delta, scores.GetValueOrDefault(x.StudentId),
                x.CreatedAt))
            .ToArray();
    }

    private Task<Dictionary<long, int>> GetScoresAsync(long yearId, long[] studentIds,
        CancellationToken cancellationToken)
    {
        return unitOfWork.ScoreTransactions.GetAll().AsNoTracking()
            .Where(x => x.AcademicYearId == yearId && studentIds.Contains(x.StudentId))
            .GroupBy(x => x.StudentId)
            .Select(g => new { StudentId = g.Key, Score = g.Sum(x => x.Delta) })
            .ToDictionaryAsync(x => x.StudentId, x => x.Score, cancellationToken);
    }

    private Task<long?> GetCurrentAcademicYearIdAsync(CancellationToken cancellationToken)
    {
        return academicYearRepository.GetAll()
            .Where(x => x.FinishedAt == null)
            .Select(x => (long?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static CollectionResult<ScoreChangedDto> StudentsNotFound(IEnumerable<long> studentIds)
    {
        return CollectionResult<ScoreChangedDto>.Failure(
            string.Format(ErrorMessage.StudentsNotFound, string.Join(", ", studentIds)),
            (int)ErrorCodes.StudentNotFound);
    }

    private static CollectionResult<ScoreChangedDto> ScoreWouldBeNegative(IEnumerable<long> studentIds)
    {
        return CollectionResult<ScoreChangedDto>.Failure(
            string.Format(ErrorMessage.ScoreWouldBeNegative, string.Join(", ", studentIds)),
            (int)ErrorCodes.ScoreWouldBeNegative);
    }
}