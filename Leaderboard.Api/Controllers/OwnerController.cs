using System.Net;
using Leaderboard.Api.Controllers.Base;
using Leaderboard.Api.Extensions;
using Leaderboard.Domain.Dtos.Auth;
using Leaderboard.Domain.Dtos.Owner;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Api.Controllers;

/// <summary>
///     Owners: any owner creates new ones, but edits only their own profile.
/// </summary>
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class OwnerController(
    IOwnerService ownerService,
    IAuthService authService) : BaseController
{
    /// <summary>
    ///     Creates an owner with a temporary password.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BaseResult<OwnerDto>>> CreateAsync(CreateOwnerDto dto,
        CancellationToken cancellationToken)
    {
        var result = await ownerService.CreateAsync(User.GetOwnerId(), dto, cancellationToken);
        return result.ToActionResult(HttpStatusCode.Created);
    }

    /// <summary>
    ///     Gets the current owner's profile.
    /// </summary>
    /// <response code="200">Profile</response>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<BaseResult<OwnerDto>>> GetMeAsync(CancellationToken cancellationToken)
    {
        var result = await ownerService.GetByIdAsync(User.GetOwnerId(), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Updates the current owner's first and last name.
    /// </summary>
    /// <response code="200">Profile updated</response>
    /// <response code="400">Invalid name</response>
    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BaseResult<OwnerDto>>> UpdateMe(UpdateOwnerDto dto,
        CancellationToken cancellationToken)
    {
        var result = await ownerService.UpdateAsync(User.GetOwnerId(), dto, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Changes the current owner's password. All sessions of the owner are signed out.
    /// </summary>
    /// <response code="204">Password changed</response>
    /// <response code="400">The current password is wrong or the new one is invalid</response>
    [HttpPut("me/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BaseResult>> ChangeMyPassword(ChangePasswordDto dto,
        CancellationToken cancellationToken)
    {
        var result = await authService.ChangePasswordAsync(User.GetOwnerId(), dto, cancellationToken);
        return result.ToActionResult();
    }
}