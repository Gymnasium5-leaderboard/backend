using AutoMapper;
using FluentValidation;
using Leaderboard.Application.Services;
using Leaderboard.Application.Validators;
using Leaderboard.Domain.Dtos.Score;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Repository;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Interfaces.Validation;
using Leaderboard.Tests.Mocks;
using Leaderboard.Tests.UnitTests.Fixtures;

namespace Leaderboard.Tests.UnitTests.Sut;

internal class ScoreServiceSut
{
    public readonly IBaseRepository<AcademicYear> AcademicYearRepository;

    public readonly IValidator<ChangeScoreBatchDto> BatchValidator =
        ValidatorFixture<ChangeScoreBatchDto>.GetValidator(
            new ChangeScoreBatchValidator(SettingsFixture.GetBusinessRules()));

    public readonly IBaseRepository<SchoolClass> ClassRepository = RepositoryMocks.GetMockClassRepository().Object;

    public readonly IValidator<ScoreHistoryQueryDto> HistoryValidator =
        ValidatorFixture<ScoreHistoryQueryDto>.GetValidator(new ScoreHistoryQueryValidator());

    public readonly IMapper Mapper = MapperFixture.GetMapperConfiguration();

    public readonly IValidator<IValidatableScore> ScoreValidator =
        ValidatorFixture<IValidatableScore>.GetValidator(new ScoreValidator(SettingsFixture.GetBusinessRules()));

    public readonly IUnitOfWork UnitOfWork = RepositoryMocks.GetMockUnitOfWork().Object;
    private readonly IScoreService _scoreService;

    public ScoreServiceSut(IBaseRepository<AcademicYear>? academicYearRepository = null,
        bool allowNegativeScore = false)
    {
        AcademicYearRepository = academicYearRepository ?? RepositoryMocks.GetMockAcademicYearRepository().Object;

        _scoreService = new ScoreService(UnitOfWork, ClassRepository, AcademicYearRepository, ScoreValidator,
            BatchValidator, HistoryValidator, SettingsFixture.GetBusinessRules(allowNegativeScore), Mapper);
    }

    public IScoreService GetService()
    {
        return _scoreService;
    }
}