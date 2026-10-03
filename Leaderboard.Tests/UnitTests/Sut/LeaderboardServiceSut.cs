using Leaderboard.Application.Services;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Repository;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Tests.Mocks;

namespace Leaderboard.Tests.UnitTests.Sut;

internal class LeaderboardServiceSut
{
    public readonly IBaseRepository<AcademicYear> AcademicYearRepository;

    public readonly IBaseRepository<SchoolClass> ClassRepository = RepositoryMocks.GetMockClassRepository().Object;

    public readonly IBaseRepository<ScoreTransaction> ScoreTransactionRepository =
        RepositoryMocks.GetMockScoreTransactionRepository().Object;

    public readonly IBaseRepository<Student> StudentRepository = RepositoryMocks.GetMockStudentRepository().Object;
    private readonly ILeaderboardService _leaderboardService;

    public LeaderboardServiceSut(IBaseRepository<AcademicYear>? academicYearRepository = null)
    {
        AcademicYearRepository = academicYearRepository ?? RepositoryMocks.GetMockAcademicYearRepository().Object;

        _leaderboardService = new LeaderboardService(StudentRepository, ClassRepository, ScoreTransactionRepository,
            AcademicYearRepository);
    }

    public ILeaderboardService GetService()
    {
        return _leaderboardService;
    }
}