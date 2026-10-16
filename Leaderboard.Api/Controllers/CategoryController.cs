using System.Net;
using Leaderboard.Api.Controllers.Base;
using Leaderboard.Api.Extensions;
using Leaderboard.Domain.Dtos.Category;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Api.Controllers;

/// <summary>
///     Categories of score changes. Anyone can read them, only owners change them.
/// </summary>
public class CategoryController(ICategoryService categoryService) : BaseController
{
    /// <summary>
    ///     Gets categories ordered by id, archived ones only if asked.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionResult<CategoryDto>>> GetAllAsync(bool includeArchived,
        CancellationToken cancellationToken)
    {
        var result = await categoryService.GetAllAsync(includeArchived, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Gets a category by id, archived ones too.
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BaseResult<CategoryDto>>> GetByIdAsync(long id,
        CancellationToken cancellationToken)
    {
        var result = await categoryService.GetByIdAsync(id, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Creates a category.
    /// </summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BaseResult<CategoryDto>>> CreateAsync(CreateCategoryDto dto,
        CancellationToken cancellationToken)
    {
        var result = await categoryService.CreateAsync(dto, cancellationToken);
        return result.ToActionResult(HttpStatusCode.Created);
    }

    /// <summary>
    ///     Renames a category, archived ones too.
    /// </summary>
    [Authorize]
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BaseResult<CategoryDto>>> UpdateAsync(long id, UpdateCategoryDto dto,
        CancellationToken cancellationToken)
    {
        var result = await categoryService.UpdateAsync(id, dto, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Archives a category: its score changes stay only in the history.
    /// </summary>
    [Authorize]
    [HttpPatch("{id:long}/archive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BaseResult<CategoryDto>>> ArchiveAsync(long id,
        CancellationToken cancellationToken)
    {
        var result = await categoryService.ArchiveAsync(id, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Returns a category from the archive.
    /// </summary>
    [Authorize]
    [HttpPatch("{id:long}/restore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BaseResult<CategoryDto>>> RestoreAsync(long id,
        CancellationToken cancellationToken)
    {
        var result = await categoryService.RestoreAsync(id, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Deletes a category without score changes in any academic year.
    /// </summary>
    [Authorize]
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BaseResult>> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var result = await categoryService.DeleteAsync(id, cancellationToken);
        return result.ToActionResult();
    }
}
