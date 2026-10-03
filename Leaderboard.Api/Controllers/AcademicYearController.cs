using Leaderboard.Api.Controllers.Base;
using Leaderboard.Api.Extensions;
using Leaderboard.Domain.Dtos.AcademicYear;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Api.Controllers;

/// <summary>
///     Academic years. Anyone can read the current one, only owners start a new one.
/// </summary>
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public class AcademicYearController(IAcademicYearService academicYearService) : BaseController
{
    /// <summary>
    ///     Gets the current academic year.
    /// </summary>
    [HttpGet("current")]
    public async Task<ActionResult<BaseResult<AcademicYearDto>>> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var result = await academicYearService.GetCurrentAsync(cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Starts a new academic year: closes the current one, graduates 11th classes and moves the other classes one
    ///     grade up. Scores start from zero. Cannot be undone.
    /// </summary>
    /// <response code="200">New year started</response>
    /// <response code="404">Current academic year not found</response>
    /// <response code="409">The year was already changed by another request</response>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BaseResult<AcademicYearDto>>> StartNewAsync(CancellationToken cancellationToken)
    {
        var result = await academicYearService.StartNewAsync(cancellationToken);
        return result.ToActionResult();
    }
}