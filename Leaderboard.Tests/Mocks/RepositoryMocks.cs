using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Database;
using Leaderboard.Domain.Interfaces.Repository;
using Leaderboard.Tests.TestData;
using MockQueryable.Moq;
using Moq;

namespace Leaderboard.Tests.Mocks;

internal static class RepositoryMocks
{
    private static readonly List<LeaderboardOwner> Owners;
    private static readonly List<SchoolClass> Classes;
    private static readonly List<Student> Students;
    private static readonly List<ScoreTransaction> Transactions;

    static RepositoryMocks()
    {
        var owners = OwnerMother.GetOwners().ToList();
        var classes = ClassMother.GetClasses().ToList();
        var students = StudentMother.GetStudents().ToList();
        var transactions = ScoreTransactionMother.GetScoreTransactions().ToList();

        foreach (var schoolClass in classes)
            schoolClass.Students = students.Where(x => x.ClassId == schoolClass.Id).ToList();

        foreach (var student in students)
            student.Class = classes.First(x => x.Id == student.ClassId);

        foreach (var transaction in transactions)
        {
            transaction.Student = students.First(x => x.Id == transaction.StudentId);
            transaction.Owner = owners.First(x => x.Id == transaction.OwnerId);
        }

        Owners = owners;
        Classes = classes;
        Students = students;
        Transactions = transactions;
    }

    public static IMock<IUnitOfWork> GetMockUnitOfWork(IBaseRepository<LeaderboardOwner>? ownerRepository = null,
        IBaseRepository<Student>? studentRepository = null,
        IBaseRepository<ScoreTransaction>? scoreTransactionRepository = null)
    {
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        mockUnitOfWork.Setup(x => x.Owners).Returns(ownerRepository ?? GetMockOwnerRepository().Object);
        mockUnitOfWork.Setup(x => x.Students).Returns(studentRepository ?? GetMockStudentRepository().Object);
        mockUnitOfWork.Setup(x => x.ScoreTransactions)
            .Returns(scoreTransactionRepository ?? GetMockScoreTransactionRepository().Object);
        mockUnitOfWork.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<ITransaction>().Object);
        mockUnitOfWork.Setup(x =>
            x.AcquireLockAsync(It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<CancellationToken>()));

        return mockUnitOfWork;
    }

    public static IMock<IBaseRepository<AcademicYear>> GetMockAcademicYearRepository()
    {
        return GetMockRepository(AcademicYearMother.GetAcademicYears());
    }

    public static IMock<IBaseRepository<LeaderboardOwner>> GetMockOwnerRepository()
    {
        return GetMockRepository(Owners);
    }

    public static IMock<IBaseRepository<RefreshToken>> GetMockRefreshTokenRepository()
    {
        // Linking tokens to owners
        var owners = Owners;
        var tokens = RefreshTokenMother.GetRefreshTokens().ToList();
        tokens.ForEach(x => x.Owner = owners.First(o => o.Id == x.OwnerId));

        return GetMockRepository(tokens);
    }

    public static IMock<IBaseRepository<SchoolClass>> GetMockClassRepository()
    {
        return GetMockRepository(Classes);
    }

    public static IMock<IBaseRepository<Student>> GetMockStudentRepository()
    {
        return GetMockRepository(Students);
    }

    public static IMock<IBaseRepository<ScoreTransaction>> GetMockScoreTransactionRepository()
    {
        return GetMockRepository(Transactions);
    }

    public static IMock<IBaseRepository<T>> GetEmptyMockRepository<T>() where T : class
    {
        return GetMockRepository(new List<T>());
    }

    private static IMock<IBaseRepository<T>> GetMockRepository<T>(IEnumerable<T> entities) where T : class
    {
        var mockRepository = new Mock<IBaseRepository<T>>();
        var dbSet = entities.BuildMockDbSet();

        mockRepository.Setup(x => x.GetAll()).Returns(dbSet.Object);
        mockRepository.Setup(x => x.CreateAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((T entity, CancellationToken _) => entity);
        mockRepository.Setup(x => x.CreateRangeAsync(It.IsAny<IEnumerable<T>>(), It.IsAny<CancellationToken>()));
        mockRepository.Setup(x => x.Update(It.IsAny<T>())).Returns((T entity) => entity);
        mockRepository.Setup(x => x.Remove(It.IsAny<T>())).Returns((T entity) => entity);

        return mockRepository;
    }
}