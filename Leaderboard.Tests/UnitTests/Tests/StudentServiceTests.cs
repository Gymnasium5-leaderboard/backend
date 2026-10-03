using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Student;
using Leaderboard.Domain.Settings;
using Leaderboard.Tests.Traits;
using Leaderboard.Tests.UnitTests.Sut;
using Xunit;

namespace Leaderboard.Tests.UnitTests.Tests;

[UnitTest]
public class StudentServiceTests
{
    [Fact]
    public async Task GetAllAsync_NoFilter_ReturnsActiveStudentsSortedByName()
    {
        //Arrange
        var studentService = new StudentServiceSut().GetService();

        //Act
        var result = await studentService.GetAllAsync(null);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal([3, 1, 2, 4, 6], result.Data.Select(x => x.Id));
    }

    [Fact]
    public async Task GetAllAsync_ClassFilter_ReturnsActiveStudentsOfClass()
    {
        //Arrange
        var studentService = new StudentServiceSut().GetService();
        const long classId = 3;

        //Act
        var result = await studentService.GetAllAsync(classId);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal([3, 4], result.Data.Select(x => x.Id));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsSuccess()
    {
        //Arrange
        var studentService = new StudentServiceSut().GetService();
        const long studentId = 1;

        //Act
        var result = await studentService.GetByIdAsync(studentId);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("11А", result.Data.ClassName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)] // left the school
    public async Task GetByIdAsync_NonExistentOrInactiveId_ReturnsStudentNotFound(long studentId)
    {
        //Arrange
        var studentService = new StudentServiceSut().GetService();

        //Act
        var result = await studentService.GetByIdAsync(studentId);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.StudentNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task CreateAsync_ValidStudent_ReturnsSuccess()
    {
        //Arrange
        var studentService = new StudentServiceSut().GetService();
        var dto = new CreateStudentDto("Nikita", "Sokolov", 3);

        //Act
        var result = await studentService.CreateAsync(dto);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Nikita", result.Data.FirstName);
    }

    [Fact]
    public async Task CreateAsync_EmptyFirstName_ReturnsInvalidFirstName()
    {
        //Arrange
        var studentService = new StudentServiceSut().GetService();
        var dto = new CreateStudentDto("", "Sokolov", 3);

        //Act
        var result = await studentService.CreateAsync(dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.InvalidFirstName, EntityConstraints.NameMaxLength),
            result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)] // graduated
    public async Task CreateAsync_NonExistentOrGraduatedClass_ReturnsClassNotFound(long classId)
    {
        //Arrange
        var studentService = new StudentServiceSut().GetService();
        var dto = new CreateStudentDto("Nikita", "Sokolov", classId);

        //Act
        var result = await studentService.CreateAsync(dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.ClassNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task CreateManyAsync_ValidStudents_ReturnsSuccess()
    {
        //Arrange
        var studentService = new StudentServiceSut().GetService();
        CreateStudentDto[] dtos = [new("Nikita", "Sokolov", 3), new("Olga", "Morozova", 4)];

        //Act
        var result = await studentService.CreateManyAsync(dtos);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task CreateManyAsync_EmptyList_ReturnsStudentListEmpty()
    {
        //Arrange
        var studentService = new StudentServiceSut().GetService();

        //Act
        var result = await studentService.CreateManyAsync([]);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.StudentListEmpty, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task CreateManyAsync_OneClassGraduated_ReturnsClassNotFound()
    {
        //Arrange
        var studentService = new StudentServiceSut().GetService();
        CreateStudentDto[] dtos = [new("Nikita", "Sokolov", 3), new("Olga", "Morozova", 5)];

        //Act
        var result = await studentService.CreateManyAsync(dtos);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.ClassNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task UpdateAsync_ExistingId_ReturnsSuccess()
    {
        //Arrange
        var studentService = new StudentServiceSut().GetService();
        const long studentId = 1;
        var dto = new UpdateStudentDto("Ivan", "Petrovsky");

        //Act
        var result = await studentService.UpdateAsync(studentId, dto);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Petrovsky", result.Data.LastName);
    }

    [Fact]
    public async Task UpdateAsync_TooLongLastName_ReturnsInvalidLastName()
    {
        //Arrange
        var studentService = new StudentServiceSut().GetService();
        const long studentId = 1;
        var dto = new UpdateStudentDto("Ivan", new string('a', EntityConstraints.NameMaxLength + 1));

        //Act
        var result = await studentService.UpdateAsync(studentId, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.InvalidLastName, EntityConstraints.NameMaxLength),
            result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task UpdateAsync_InactiveId_ReturnsStudentNotFound()
    {
        //Arrange
        var studentService = new StudentServiceSut().GetService();
        const long studentId = 5;
        var dto = new UpdateStudentDto("Petr", "Kozlov");

        //Act
        var result = await studentService.UpdateAsync(studentId, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.StudentNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task TransferAsync_ActiveClass_ReturnsSuccess()
    {
        //Arrange
        var studentService = new StudentServiceSut().GetService();
        const long studentId = 3;
        var dto = new TransferStudentDto(4);

        //Act
        var result = await studentService.TransferAsync(studentId, dto);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("7Б", result.Data.ClassName);
    }

    [Fact]
    public async Task TransferAsync_GraduatedClass_ReturnsClassNotFound()
    {
        //Arrange
        var studentService = new StudentServiceSut().GetService();
        const long studentId = 3;
        var dto = new TransferStudentDto(5);

        //Act
        var result = await studentService.TransferAsync(studentId, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.ClassNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task TransferAsync_NonExistentStudent_ReturnsStudentNotFound()
    {
        //Arrange
        var studentService = new StudentServiceSut().GetService();
        const long studentId = 0;
        var dto = new TransferStudentDto(4);

        //Act
        var result = await studentService.TransferAsync(studentId, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.StudentNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task DeactivateAsync_ActiveStudent_ReturnsSuccess()
    {
        //Arrange
        var studentService = new StudentServiceSut().GetService();
        const long studentId = 1;

        //Act
        var result = await studentService.DeactivateAsync(studentId);

        //Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeactivateAsync_InactiveStudent_ReturnsStudentNotFound()
    {
        //Arrange
        var studentService = new StudentServiceSut().GetService();
        const long studentId = 5;

        //Act
        var result = await studentService.DeactivateAsync(studentId);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.StudentNotFound, result.ErrorMessage);
    }
}