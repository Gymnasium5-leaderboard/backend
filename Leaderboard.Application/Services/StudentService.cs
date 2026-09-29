using AutoMapper;
using FluentValidation;
using Leaderboard.Application.Enums;
using Leaderboard.Application.Extensions;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Student;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Repository;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Interfaces.Validation;
using Leaderboard.Domain.Results;
using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Application.Services;

public class StudentService(
    IBaseRepository<Student> studentRepository,
    IBaseRepository<SchoolClass> classRepository,
    IValidator<IValidatableName> nameValidator,
    IMapper mapper) : IStudentService
{
    public async Task<CollectionResult<StudentDto>> GetAllAsync(long? classId,
        CancellationToken cancellationToken = default)
    {
        var students = await studentRepository.GetAll().AsNoTracking()
            .Include(x => x.Class)
            .Where(x => x.IsActive && (classId == null || x.ClassId == classId))
            .OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .ToArrayAsync(cancellationToken);

        return CollectionResult<StudentDto>.Success(mapper.Map<StudentDto[]>(students));
    }

    public async Task<BaseResult<StudentDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var student = await studentRepository.GetAll().AsNoTracking()
            .Include(x => x.Class)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);

        if (student == null) return StudentNotFound();

        return BaseResult<StudentDto>.Success(mapper.Map<StudentDto>(student));
    }

    public async Task<BaseResult<StudentDto>> CreateAsync(CreateStudentDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await CreateManyAsync([dto], cancellationToken);
        if (!result.IsSuccess) return BaseResult<StudentDto>.Failure(result.ErrorMessage!, result.ErrorCode);

        return BaseResult<StudentDto>.Success(result.Data.Single());
    }

    public async Task<CollectionResult<StudentDto>> CreateManyAsync(IReadOnlyCollection<CreateStudentDto> dtos,
        CancellationToken cancellationToken = default)
    {
        if (dtos.Count == 0)
            return CollectionResult<StudentDto>.Failure(ErrorMessage.StudentListEmpty, (int)ErrorCodes.InvalidProperty);

        foreach (var dto in dtos)
        {
            var (isValid, errorMessage) = await nameValidator.ValidateWithMessageAsync(dto, cancellationToken);
            if (!isValid) return CollectionResult<StudentDto>.Failure(errorMessage, (int)ErrorCodes.InvalidProperty);
        }

        // Tracked, so EF fills Student.Class for the response
        var classIds = dtos.Select(x => x.ClassId).Distinct().ToArray();
        var classes = await classRepository.GetAll()
            .Where(x => classIds.Contains(x.Id) && x.IsActive)
            .ToArrayAsync(cancellationToken);
        if (classes.Length != classIds.Length)
            return CollectionResult<StudentDto>.Failure(ErrorMessage.ClassNotFound, (int)ErrorCodes.ClassNotFound);

        var students = mapper.Map<Student[]>(dtos);
        foreach (var student in students) await studentRepository.CreateAsync(student, cancellationToken);
        await studentRepository.SaveChangesAsync(cancellationToken);

        return CollectionResult<StudentDto>.Success(mapper.Map<StudentDto[]>(students));
    }

    public async Task<BaseResult<StudentDto>> UpdateAsync(long id, UpdateStudentDto dto,
        CancellationToken cancellationToken = default)
    {
        var (isValid, errorMessage) = await nameValidator.ValidateWithMessageAsync(dto, cancellationToken);
        if (!isValid) return BaseResult<StudentDto>.Failure(errorMessage, (int)ErrorCodes.InvalidProperty);

        var student = await GetActiveStudentAsync(id, cancellationToken);
        if (student == null) return StudentNotFound();

        mapper.Map(dto, student);
        await studentRepository.SaveChangesAsync(cancellationToken);

        return BaseResult<StudentDto>.Success(mapper.Map<StudentDto>(student));
    }

    public async Task<BaseResult<StudentDto>> TransferAsync(long id, TransferStudentDto dto,
        CancellationToken cancellationToken = default)
    {
        var student = await GetActiveStudentAsync(id, cancellationToken);
        if (student == null) return StudentNotFound();

        var schoolClass = await classRepository.GetAll()
            .FirstOrDefaultAsync(x => x.Id == dto.ClassId && x.IsActive, cancellationToken);
        if (schoolClass == null)
            return BaseResult<StudentDto>.Failure(ErrorMessage.ClassNotFound, (int)ErrorCodes.ClassNotFound);

        // Scores are linked to the student, not the class, so they move with the student
        student.Class = schoolClass;
        await studentRepository.SaveChangesAsync(cancellationToken);

        return BaseResult<StudentDto>.Success(mapper.Map<StudentDto>(student));
    }

    public async Task<BaseResult> DeactivateAsync(long id, CancellationToken cancellationToken = default)
    {
        var student = await GetActiveStudentAsync(id, cancellationToken);
        if (student == null)
            return BaseResult.Failure(ErrorMessage.StudentNotFound, (int)ErrorCodes.StudentNotFound);

        student.IsActive = false;
        await studentRepository.SaveChangesAsync(cancellationToken);

        return BaseResult.Success();
    }

    private Task<Student?> GetActiveStudentAsync(long id, CancellationToken cancellationToken) =>
        studentRepository.GetAll()
            .Include(x => x.Class)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);

    private static BaseResult<StudentDto> StudentNotFound() =>
        BaseResult<StudentDto>.Failure(ErrorMessage.StudentNotFound, (int)ErrorCodes.StudentNotFound);
}