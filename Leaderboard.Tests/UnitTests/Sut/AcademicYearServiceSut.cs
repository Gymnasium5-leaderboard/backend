using AutoMapper;
using Leaderboard.Application.Services;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Repository;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Tests.Mocks;
using Leaderboard.Tests.UnitTests.Fixtures;

namespace Leaderboard.Tests.UnitTests.Sut;

internal class AcademicYearServiceSut
{
    public readonly IBaseRepository<AcademicYear> AcademicYearRepository;

    public readonly IBaseRepository<SchoolClass> ClassRepository = RepositoryMocks.GetMockClassRepository().Object;

    public readonly IMapper Mapper = MapperFixture.GetMapperConfiguration();

    public readonly IUnitOfWork UnitOfWork = RepositoryMocks.GetMockUnitOfWork().Object;

    // One class implements both interfaces
    private readonly AcademicYearService _academicYearService;

    public AcademicYearServiceSut(IBaseRepository<AcademicYear>? academicYearRepository = null)
    {
        AcademicYearRepository = academicYearRepository ?? RepositoryMocks.GetMockAcademicYearRepository().Object;

        _academicYearService = new AcademicYearService(UnitOfWork, ClassRepository, AcademicYearRepository, Mapper);
    }

    public IAcademicYearService GetService()
    {
        return _academicYearService;
    }

    public IAcademicYearInitializer GetInitializer()
    {
        return _academicYearService;
    }
}