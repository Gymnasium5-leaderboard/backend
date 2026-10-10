using System.Net;
using Leaderboard.Api.Controllers.Base;
using Leaderboard.Api.Extensions;
using Leaderboard.Domain.Dtos.Class;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Api.Controllers;

/// <summary>
///     School classes 4-11. Anyone can read them, only owners change them.
/// </summary>
public class ClassController(IClassService classService) : BaseController
{
    /// <summary>
    ///     Gets active classes, optionally of one grade.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionResult<ClassDto>>> GetAllAsync(int? grade,
        CancellationToken cancellationToken)
    {
        var result = await classService.GetAllAsync(grade, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Gets an active class by id.
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BaseResult<ClassDto>>> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var result = await classService.GetByIdAsync(id, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Creates a class.
    /// </summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BaseResult<ClassDto>>> CreateAsync(CreateClassDto dto,
        CancellationToken cancellationToken)
    {
        var result = await classService.CreateAsync(dto, cancellationToken);
        return result.ToActionResult(HttpStatusCode.Created);
    }

    /// <summary>
    ///     Changes the grade or letter of an active class.
    /// </summary>
    [Authorize]
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BaseResult<ClassDto>>> UpdateAsync(long id, UpdateClassDto dto,
        CancellationToken cancellationToken)
    {
        var result = await classService.UpdateAsync(id, dto, cancellationToken);
        return result.ToActionResult();
    }
}