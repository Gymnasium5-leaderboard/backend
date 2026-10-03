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

internal class ClassServiceSut
{
    public readonly IBaseRepository<SchoolClass> ClassRepository = RepositoryMocks.GetMockClassRepository().Object;

    public readonly IMapper Mapper = MapperFixture.GetMapperConfiguration();

    public readonly IValidator<IValidatableClass> Validator =
        ValidatorFixture<IValidatableClass>.GetValidator(new ClassValidator());

    private readonly IClassService _classService;

    public ClassServiceSut()
    {
        _classService = new ClassService(ClassRepository, Validator, Mapper);
    }

    public IClassService GetService()
    {
        return _classService;
    }
}