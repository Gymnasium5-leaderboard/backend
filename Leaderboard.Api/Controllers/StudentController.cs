using System.Net;
using Leaderboard.Api.Controllers.Base;
using Leaderboard.Api.Extensions;
using Leaderboard.Domain.Dtos.Student;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Api.Controllers;

/// <summary>
///     Students. Only owners see and change them.
/// </summary>
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class StudentController(IStudentService studentService) : BaseController
{
    /// <summary>
    ///     Gets active students, optionally of one class.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionResult<StudentDto>>> GetAllAsync(long? classId,
        CancellationToken cancellationToken)
    {
        var result = await studentService.GetAllAsync(classId, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Gets an active student by id.
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BaseResult<StudentDto>>> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var result = await studentService.GetByIdAsync(id, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Adds a student.
    /// </summary>
    /// <response code="201">Student added</response>
    /// <response code="400">Invalid name</response>
    /// <response code="404">Class not found or graduated</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BaseResult<StudentDto>>> CreateAsync(CreateStudentDto dto,
        CancellationToken cancellationToken)
    {
        var result = await studentService.CreateAsync(dto, cancellationToken);
        return result.ToActionResult(HttpStatusCode.Created);
    }

    /// <summary>
    ///     Adds a list of students. If any of them is invalid, none is added.
    /// </summary>
    /// <response code="201">Students added</response>
    /// <response code="400">Empty list or invalid name</response>
    /// <response code="404">Class not found or graduated</response>
    [HttpPost("batch")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CollectionResult<StudentDto>>> CreateManyAsync(
        IReadOnlyCollection<CreateStudentDto> dtos, CancellationToken cancellationToken)
    {
        var result = await studentService.CreateManyAsync(dtos, cancellationToken);
        return result.ToActionResult(HttpStatusCode.Created);
    }

    /// <summary>
    ///     Changes the first and last name of an active student.
    /// </summary>
    /// <response code="200">Student updated</response>
    /// <response code="400">Invalid name</response>
    /// <response code="404">Student not found or left the school</response>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BaseResult<StudentDto>>> UpdateAsync(long id, UpdateStudentDto dto,
        CancellationToken cancellationToken)
    {
        var result = await studentService.UpdateAsync(id, dto, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Moves an active student to another class. The student keeps their scores.
    /// </summary>
    /// <response code="200">Student moved</response>
    /// <response code="404">Student or class not found</response>
    [HttpPatch("{id:long}/transfer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BaseResult<StudentDto>>> TransferAsync(long id, TransferStudentDto dto,
        CancellationToken cancellationToken)
    {
        var result = await studentService.TransferAsync(id, dto, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Marks a student as left the school. The student leaves the leaderboards, the score history is kept.
    /// </summary>
    /// <response code="204">Student deactivated</response>
    /// <response code="404">Student not found or already left the school</response>
    [HttpPatch("{id:long}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BaseResult>> DeactivateAsync(long id, CancellationToken cancellationToken)
    {
        var result = await studentService.DeactivateAsync(id, cancellationToken);
        return result.ToActionResult();
    }
}