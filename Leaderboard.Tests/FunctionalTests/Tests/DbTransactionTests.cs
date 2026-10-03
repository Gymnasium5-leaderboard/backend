using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Repository;
using Leaderboard.Tests.FunctionalTests.Base;
using Leaderboard.Tests.Traits;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Leaderboard.Tests.FunctionalTests.Tests;

[FunctionalTest]
public class DbTransactionTests(FunctionalTestWebAppFactory factory) : SequentialFunctionalTest(factory)
{
    [Fact]
    public async Task CommitTransaction_SingleTransaction_ReturnsCommitted()
    {
        //Arrange
        await using var scope = ServiceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        //Act
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        await unitOfWork.Students.CreateAsync(new Student { FirstName = "Nikita", LastName = "Sokolov", ClassId = 3 });
        await unitOfWork.SaveChangesAsync();
        await transaction.CommitAsync();

        //Assert
        var count = await unitOfWork.Students.GetAll().AsNoTracking().CountAsync();
        Assert.Equal(8, count);
    }

    [Fact]
    public async Task RollbackTransaction_SingleTransaction_ReturnsRolledBack()
    {
        //Arrange
        await using var scope = ServiceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        //Act
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        await unitOfWork.Students.CreateAsync(new Student { FirstName = "Nikita", LastName = "Sokolov", ClassId = 3 });
        await unitOfWork.SaveChangesAsync();
        await transaction.RollbackAsync();

        //Assert
        var count = await unitOfWork.Students.GetAll().AsNoTracking().CountAsync();
        Assert.Equal(7, count);
    }

    [Fact]
    public async Task DisposeTransaction_SingleTransaction_ReturnsRolledBack()
    {
        //Arrange
        await using var scope = ServiceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        //Act
        var transaction = await unitOfWork.BeginTransactionAsync();
        await unitOfWork.Students.CreateAsync(new Student { FirstName = "Nikita", LastName = "Sokolov", ClassId = 3 });
        await unitOfWork.SaveChangesAsync();
        await transaction.DisposeAsync();

        //Assert
        var count = await unitOfWork.Students.GetAll().AsNoTracking().CountAsync();
        Assert.Equal(7, count);
    }

    [Fact]
    public async Task AcquireLock_NoTransaction_ThrowsInvalidOperationException()
    {
        //Arrange
        await using var scope = ServiceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        //Act
        var action = () => unitOfWork.AcquireLockAsync([1]);

        //Assert
        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }
}