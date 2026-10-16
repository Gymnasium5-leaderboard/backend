using AutoMapper;
using FluentValidation;
using Leaderboard.Application.Services;
using Leaderboard.Application.Validators;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Repository;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Interfaces.Validation;
using Leaderboard.Tests.Mocks;
using Leaderboard.Tests.UnitTests.Fixtures;

namespace Leaderboard.Tests.UnitTests.Sut;

internal class CategoryServiceSut
{
    public readonly IBaseRepository<Category> CategoryRepository =
        RepositoryMocks.GetMockCategoryRepository().Object;

    public readonly IMapper Mapper = MapperFixture.GetMapperConfiguration();

    public readonly IBaseRepository<ScoreTransaction> ScoreTransactionRepository =
        RepositoryMocks.GetMockScoreTransactionRepository().Object;

    public readonly IValidator<IValidatableCategory> Validator =
        ValidatorFixture<IValidatableCategory>.GetValidator(new CategoryValidator());

    private readonly ICategoryService _categoryService;

    public CategoryServiceSut()
    {
        _categoryService = new CategoryService(CategoryRepository, ScoreTransactionRepository, Validator, Mapper);
    }

    public ICategoryService GetService()
    {
        return _categoryService;
    }
}
