using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Leaderboard.Domain.Results;

/// <summary>
///     Result of an operation: success, or failure with an error message and optional code.
/// </summary>
public class BaseResult
{
    [JsonConstructor]
    protected BaseResult()
    {
    }

    protected BaseResult(string errorMessage, int? errorCode = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(errorMessage);

        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    /// <summary>
    ///     True if <see cref="ErrorMessage" /> is <c>null</c>.
    /// </summary>
    public bool IsSuccess => ErrorMessage == null;

    [JsonInclude] public string? ErrorMessage { get; private init; }

    [JsonInclude] public int? ErrorCode { get; private init; }

    public static BaseResult Success()
    {
        return new BaseResult();
    }

    public static BaseResult Failure(string errorMessage, int? errorCode = null)
    {
        return new BaseResult(errorMessage, errorCode);
    }
}

/// <summary>
///     Result of an operation that returns <typeparamref name="T" /> on success.
/// </summary>
public class BaseResult<T> : BaseResult where T : class
{
    [JsonConstructor]
    protected BaseResult()
    {
    }

    protected BaseResult(T data)
    {
        ArgumentNullException.ThrowIfNull(data);

        Data = data;
    }

    protected BaseResult(string errorMessage, int? errorCode = null) : base(errorMessage, errorCode)
    {
    }

    /// <inheritdoc cref="BaseResult.IsSuccess" />
    [MemberNotNullWhen(true, nameof(Data))]
    public new bool IsSuccess => base.IsSuccess;

    [JsonInclude] public T? Data { get; private init; }

    public static BaseResult<T> Success(T data)
    {
        return new BaseResult<T>(data);
    }

    public new static BaseResult<T> Failure(string errorMessage, int? errorCode = null)
    {
        return new BaseResult<T>(errorMessage, errorCode);
    }
}
