using System.Text.Json.Serialization;

namespace Leaderboard.Domain.Results;

/// <summary>
///     Result of an operation that returns a collection of <typeparamref name="T" />.
/// </summary>
public class CollectionResult<T> : BaseResult<IReadOnlyCollection<T>>
{
    [JsonConstructor]
    protected CollectionResult()
    {
    }

    protected CollectionResult(IReadOnlyCollection<T> data) : base(data)
    {
    }

    protected CollectionResult(string errorMessage, int? errorCode = null) : base(errorMessage, errorCode)
    {
    }

    public int Count => Data?.Count ?? 0;

    public new static CollectionResult<T> Success(IReadOnlyCollection<T> data)
    {
        return new CollectionResult<T>(data);
    }

    public new static CollectionResult<T> Failure(string errorMessage, int? errorCode = null)
    {
        return new CollectionResult<T>(errorMessage, errorCode);
    }
}
