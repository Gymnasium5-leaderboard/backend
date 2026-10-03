using Leaderboard.DAL.Repositories;
using Leaderboard.Domain.Interfaces.Database;
using Leaderboard.Domain.Interfaces.Repository;
using Leaderboard.Tests.Support;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace Leaderboard.Tests.FunctionalTests.Base.Exception;

public class ExceptionFunctionalTestWebAppFactory : FunctionalTestWebAppFactory
{
    private static IMock<ITransaction> GetExceptionMockTransaction(ITransaction originalTransaction)
    {
        var mockTransaction = new Mock<ITransaction>();

        mockTransaction.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(originalTransaction.RollbackAsync);
        mockTransaction.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new TestException());
        mockTransaction.Setup(x => x.DisposeAsync()).Returns(originalTransaction.DisposeAsync);

        return mockTransaction;
    }

    private static IMock<IUnitOfWork> GetExceptionMockUnitOfWork(IUnitOfWork originalUnitOfWork)
    {
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(originalUnitOfWork.SaveChangesAsync);
        mockUnitOfWork.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken cancellationToken) =>
                GetExceptionMockTransaction(await originalUnitOfWork.BeginTransactionAsync(cancellationToken))
                    .Object);
        mockUnitOfWork.Setup(x =>
                x.AcquireLockAsync(It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<CancellationToken>()))
            .Returns(originalUnitOfWork.AcquireLockAsync);
        mockUnitOfWork.Setup(x => x.Owners).Returns(originalUnitOfWork.Owners);
        mockUnitOfWork.Setup(x => x.Students).Returns(originalUnitOfWork.Students);
        mockUnitOfWork.Setup(x => x.ScoreTransactions).Returns(originalUnitOfWork.ScoreTransactions);

        return mockUnitOfWork;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(provider =>
            {
                //the dependencies from service provider only apply for this current scope
                //that is why we have to use ActivatorUtilities to transfer dependencies from this scope to callers' scope
                var unitOfWork = ActivatorUtilities.CreateInstance<UnitOfWork>(provider);
                return GetExceptionMockUnitOfWork(unitOfWork).Object;
            });
        });
    }
}