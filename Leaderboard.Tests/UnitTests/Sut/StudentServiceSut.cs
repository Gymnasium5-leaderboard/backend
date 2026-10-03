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

internal class StudentServiceSut
{
    public readonly IBaseRepository<SchoolClass> ClassRepository = RepositoryMocks.GetMockClassRepository().Object;

    public readonly IMapper Mapper = MapperFixture.GetMapperConfiguration();

    public readonly IValidator<IValidatableName> NameValidator =
        ValidatorFixture<IValidatableName>.GetValidator(new NameValidator());

    public readonly IBaseRepository<Student> StudentRepository = RepositoryMocks.GetMockStudentRepository().Object;
    private readonly IStudentService _studentService;

    public StudentServiceSut()
    {
        _studentService = new StudentService(StudentRepository, ClassRepository, NameValidator, Mapper);
    }

    public IStudentService GetService()
    {
        return _studentService;
    }
}