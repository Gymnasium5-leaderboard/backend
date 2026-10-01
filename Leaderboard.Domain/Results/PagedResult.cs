using System.Text.Json.Serialization;

namespace Leaderboard.Domain.Results;

/// <summary>
///     One page of a collection and the total number of items in it.
/// </summary>
public class PagedResult<T> : CollectionResult<T>
{
    [JsonConstructor]
    protected PagedResult()
    {
    }

    protected PagedResult(IReadOnlyCollection<T> data, int totalCount, int page, int pageSize) : base(data)
    {
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    protected PagedResult(string errorMessage, int? errorCode = null) : base(errorMessage, errorCode)
    {
    }

    [JsonInclude] public int TotalCount { get; private init; }

    [JsonInclude] public int Page { get; private init; }

    [JsonInclude] public int PageSize { get; private init; }

    public static PagedResult<T> Success(IReadOnlyCollection<T> data, int totalCount, int page, int pageSize)
    {
        return new PagedResult<T>(data, totalCount, page, pageSize);
    }

    public new static PagedResult<T> Failure(string errorMessage, int? errorCode = null)
    {
        return new PagedResult<T>(errorMessage, errorCode);
    }
}