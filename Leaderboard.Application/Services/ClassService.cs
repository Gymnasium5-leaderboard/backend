using AutoMapper;
using FluentValidation;
using Leaderboard.Application.Enums;
using Leaderboard.Application.Extensions;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Class;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Repository;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Interfaces.Validation;
using Leaderboard.Domain.Results;
using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Application.Services;

public class ClassService(
    IBaseRepository<SchoolClass> classRepository,
    IValidator<IValidatableClass> validator,
    IMapper mapper) : IClassService
{
    public async Task<CollectionResult<ClassDto>> GetAllAsync(int? grade,
        CancellationToken cancellationToken = default)
    {
        var classes = await classRepository.GetAll().AsNoTracking()
            .Where(x => x.IsActive && (grade == null || x.Grade == grade))
            .OrderBy(x => x.Grade).ThenBy(x => x.Letter)
            .ToArrayAsync(cancellationToken);

        return CollectionResult<ClassDto>.Success(mapper.Map<ClassDto[]>(classes));
    }

    public async Task<BaseResult<ClassDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var schoolClass = await classRepository.GetAll().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);

        if (schoolClass == null) return ClassNotFound();

        return BaseResult<ClassDto>.Success(mapper.Map<ClassDto>(schoolClass));
    }

    public async Task<BaseResult<ClassDto>> CreateAsync(CreateClassDto dto,
        CancellationToken cancellationToken = default)
    {
        var (isValid, errorMessage) = await validator.ValidateWithMessageAsync(dto, cancellationToken);
        if (!isValid) return BaseResult<ClassDto>.Failure(errorMessage, (int)ErrorCodes.InvalidProperty);

        // "7а" and "7А" are the same class
        dto = dto with { Letter = char.ToUpperInvariant(dto.Letter) };
        if (await classRepository.GetAll().AnyAsync(x => x.IsActive && x.Grade == dto.Grade && x.Letter == dto.Letter,
                cancellationToken)) return ClassAlreadyExists();

        var schoolClass = mapper.Map<SchoolClass>(dto);
        await classRepository.CreateAsync(schoolClass, cancellationToken);
        await classRepository.SaveChangesAsync(cancellationToken);

        return BaseResult<ClassDto>.Success(mapper.Map<ClassDto>(schoolClass));
    }

    public async Task<BaseResult<ClassDto>> UpdateAsync(long id, UpdateClassDto dto,
        CancellationToken cancellationToken = default)
    {
        var (isValid, errorMessage) = await validator.ValidateWithMessageAsync(dto, cancellationToken);
        if (!isValid) return BaseResult<ClassDto>.Failure(errorMessage, (int)ErrorCodes.InvalidProperty);

        // Graduated classes are history and are not edited
        var schoolClass = await classRepository.GetAll()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);
        if (schoolClass == null) return ClassNotFound();

        dto = dto with { Letter = char.ToUpperInvariant(dto.Letter) };
        if (await classRepository.GetAll()
                .AnyAsync(x => x.IsActive && x.Grade == dto.Grade && x.Letter == dto.Letter && x.Id != id,
                    cancellationToken)) return ClassAlreadyExists();

        mapper.Map(dto, schoolClass);
        await classRepository.SaveChangesAsync(cancellationToken);

        return BaseResult<ClassDto>.Success(mapper.Map<ClassDto>(schoolClass));
    }

    private static BaseResult<ClassDto> ClassNotFound() =>
        BaseResult<ClassDto>.Failure(ErrorMessage.ClassNotFound, (int)ErrorCodes.ClassNotFound);

    private static BaseResult<ClassDto> ClassAlreadyExists() =>
        BaseResult<ClassDto>.Failure(ErrorMessage.ClassAlreadyExists, (int)ErrorCodes.ClassAlreadyExists);
}