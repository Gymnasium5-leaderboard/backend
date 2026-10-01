using System.Net;
using Leaderboard.Application.Enums;
using Leaderboard.Domain.Results;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Api.Extensions;

public static class BaseResultExtensions
{
    private static readonly IReadOnlyDictionary<int, int> ErrorStatusCodeMap = new Dictionary<int, int>
    {
        // Data
        { (int)ErrorCodes.InvalidProperty, StatusCodes.Status400BadRequest },

        // Auth
        { (int)ErrorCodes.InvalidCredentials, StatusCodes.Status401Unauthorized },
        { (int)ErrorCodes.InvalidRefreshToken, StatusCodes.Status401Unauthorized },
        { (int)ErrorCodes.WrongCurrentPassword, StatusCodes.Status401Unauthorized },
        { (int)ErrorCodes.InvalidClaims, StatusCodes.Status401Unauthorized },

        // Owner
        { (int)ErrorCodes.OwnerNotFound, StatusCodes.Status404NotFound },
        { (int)ErrorCodes.OwnerAlreadyExists, StatusCodes.Status409Conflict },

        // AcademicYear
        { (int)ErrorCodes.CurrentAcademicYearNotFound, StatusCodes.Status404NotFound },

        // Class
        { (int)ErrorCodes.ClassNotFound, StatusCodes.Status404NotFound },
        { (int)ErrorCodes.ClassAlreadyExists, StatusCodes.Status409Conflict },

        // Student
        { (int)ErrorCodes.StudentNotFound, StatusCodes.Status404NotFound },

        // Score
        { (int)ErrorCodes.ScoreWouldBeNegative, StatusCodes.Status409Conflict }
    };

    /// <summary>
    ///     Converts a <see cref="BaseResult{T}" /> into an ActionResult with the status code of its error.
    /// </summary>
    public static ActionResult<BaseResult<T>> ToActionResult<T>(this BaseResult<T> result,
        HttpStatusCode successStatusCode = HttpStatusCode.OK) where T : class
    {
        if (result.IsSuccess) return new ObjectResult(result) { StatusCode = (int)successStatusCode };

        return new ObjectResult(result) { StatusCode = GetStatusCode(result.ErrorCode) };
    }

    /// <summary>
    ///     Converts a <see cref="BaseResult" /> into an ActionResult: 204 on success, the error status code otherwise.
    /// </summary>
    public static ActionResult<BaseResult> ToActionResult(this BaseResult result)
    {
        if (result.IsSuccess) return new StatusCodeResult(StatusCodes.Status204NoContent);

        return new ObjectResult(result) { StatusCode = GetStatusCode(result.ErrorCode) };
    }

    /// <summary>
    ///     Converts a <see cref="CollectionResult{T}" /> into an ActionResult with the status code of its error.
    /// </summary>
    public static ActionResult<CollectionResult<T>> ToActionResult<T>(
        this CollectionResult<T> result,
        HttpStatusCode successStatusCode = HttpStatusCode.OK) where T : class
    {
        if (result.IsSuccess) return new ObjectResult(result) { StatusCode = (int)successStatusCode };

        return new ObjectResult(result) { StatusCode = GetStatusCode(result.ErrorCode) };
    }

    private static int GetStatusCode(int? errorCode)
    {
        if (errorCode != null && ErrorStatusCodeMap.TryGetValue((int)errorCode, out var code)) return code;

        return StatusCodes.Status500InternalServerError;
    }
}