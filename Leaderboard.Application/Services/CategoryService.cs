using AutoMapper;
using EntityFramework.Exceptions.Common;
using FluentValidation;
using Leaderboard.Application.Enums;
using Leaderboard.Application.Extensions;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Category;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Repository;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Interfaces.Validation;
using Leaderboard.Domain.Results;
using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Application.Services;

public class CategoryService(
    IBaseRepository<Category> categoryRepository,
    IBaseRepository<ScoreTransaction> scoreTransactionRepository,
    IValidator<IValidatableCategory> validator,
    IMapper mapper) : ICategoryService
{
    public async Task<CollectionResult<CategoryDto>> GetAllAsync(bool includeArchived,
        CancellationToken cancellationToken = default)
    {
        var categories = await categoryRepository.GetAll().AsNoTracking()
            .Where(x => includeArchived || !x.IsArchived)
            .OrderBy(x => x.Id)
            .ToArrayAsync(cancellationToken);

        return CollectionResult<CategoryDto>.Success(mapper.Map<CategoryDto[]>(categories));
    }

    public async Task<BaseResult<CategoryDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetAll().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category == null) return CategoryNotFound();

        return BaseResult<CategoryDto>.Success(mapper.Map<CategoryDto>(category));
    }

    public async Task<BaseResult<CategoryDto>> CreateAsync(CreateCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        var (isValid, errorMessage) = await validator.ValidateWithMessageAsync(dto, cancellationToken);
        if (!isValid) return BaseResult<CategoryDto>.Failure(errorMessage, (int)ErrorCodes.InvalidProperty);

        dto = new CreateCategoryDto(dto.Name.Trim());
        if (await NameExistsAsync(dto.Name, null, cancellationToken)) return CategoryAlreadyExists();

        var category = mapper.Map<Category>(dto);
        await categoryRepository.CreateAsync(category, cancellationToken);

        await categoryRepository.SaveChangesAsync(cancellationToken);

        return BaseResult<CategoryDto>.Success(mapper.Map<CategoryDto>(category));
    }

    public async Task<BaseResult<CategoryDto>> UpdateAsync(long id, UpdateCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        var (isValid, errorMessage) = await validator.ValidateWithMessageAsync(dto, cancellationToken);
        if (!isValid) return BaseResult<CategoryDto>.Failure(errorMessage, (int)ErrorCodes.InvalidProperty);

        var category = await categoryRepository.GetAll().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category == null) return CategoryNotFound();

        dto = new UpdateCategoryDto(dto.Name.Trim());
        if (await NameExistsAsync(dto.Name, id, cancellationToken)) return CategoryAlreadyExists();

        mapper.Map(dto, category);

        await categoryRepository.SaveChangesAsync(cancellationToken);

        return BaseResult<CategoryDto>.Success(mapper.Map<CategoryDto>(category));
    }

    public Task<BaseResult<CategoryDto>> ArchiveAsync(long id, CancellationToken cancellationToken = default) =>
        SetArchivedAsync(id, true, cancellationToken);

    public Task<BaseResult<CategoryDto>> RestoreAsync(long id, CancellationToken cancellationToken = default) =>
        SetArchivedAsync(id, false, cancellationToken);

    public async Task<BaseResult> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetAll().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category == null)
            return BaseResult.Failure(ErrorMessage.CategoryNotFound, (int)ErrorCodes.CategoryNotFound);

        // Transactions of any academic year keep the category
        if (await scoreTransactionRepository.GetAll().AnyAsync(x => x.CategoryId == id, cancellationToken))
            return CategoryInUse();

        categoryRepository.Remove(category);

        await categoryRepository.SaveChangesAsync(cancellationToken);

        return BaseResult.Success();
    }

    private async Task<BaseResult<CategoryDto>> SetArchivedAsync(long id, bool isArchived,
        CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetAll().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category == null) return CategoryNotFound();

        if (category.IsArchived != isArchived)
        {
            category.IsArchived = isArchived;
            await categoryRepository.SaveChangesAsync(cancellationToken);
        }

        return BaseResult<CategoryDto>.Success(mapper.Map<CategoryDto>(category));
    }

    private Task<bool> NameExistsAsync(string name, long? exceptId, CancellationToken cancellationToken)
    {
        var lowerName = name.ToLower();
        return categoryRepository.GetAll()
            .AnyAsync(x => x.Name.ToLower() == lowerName && x.Id != exceptId, cancellationToken);
    }

    private static BaseResult<CategoryDto> CategoryNotFound() =>
        BaseResult<CategoryDto>.Failure(ErrorMessage.CategoryNotFound, (int)ErrorCodes.CategoryNotFound);

    private static BaseResult<CategoryDto> CategoryAlreadyExists() =>
        BaseResult<CategoryDto>.Failure(ErrorMessage.CategoryAlreadyExists, (int)ErrorCodes.CategoryAlreadyExists);

    private static BaseResult CategoryInUse() =>
        BaseResult.Failure(ErrorMessage.CategoryInUse, (int)ErrorCodes.CategoryInUse);
}