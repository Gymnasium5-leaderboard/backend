using AutoMapper;
using FluentValidation;
using Leaderboard.Application.Services;
using Leaderboard.Application.Validators;
using Leaderboard.Domain.Dtos.Owner;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Repository;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Interfaces.Validation;
using Leaderboard.Tests.Mocks;
using Leaderboard.Tests.UnitTests.Fixtures;
using Microsoft.AspNetCore.Identity;

namespace Leaderboard.Tests.UnitTests.Sut;

internal class OwnerServiceSut
{
    public readonly IValidator<CreateOwnerDto> CreateValidator =
        ValidatorFixture<CreateOwnerDto>.GetValidator(new CreateOwnerValidator());

    public readonly IMapper Mapper = MapperFixture.GetMapperConfiguration();

    public readonly IValidator<IValidatableName> NameValidator =
        ValidatorFixture<IValidatableName>.GetValidator(new NameValidator());

    public readonly IBaseRepository<LeaderboardOwner> OwnerRepository =
        RepositoryMocks.GetMockOwnerRepository().Object;

    public readonly IPasswordHasher<LeaderboardOwner> PasswordHasher = new PasswordHasher<LeaderboardOwner>();
    private readonly IOwnerService _ownerService;

    public OwnerServiceSut()
    {
        _ownerService = new OwnerService(OwnerRepository, PasswordHasher, CreateValidator, NameValidator, Mapper);
    }

    public IOwnerService GetService()
    {
        return _ownerService;
    }
}