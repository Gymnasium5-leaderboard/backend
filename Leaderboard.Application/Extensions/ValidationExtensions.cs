using FluentValidation;

namespace Leaderboard.Application.Extensions;

public static class ValidationExtensions
{
    /// <summary>
    ///     Validates the value and joins all validation errors into one message.
    /// </summary>
    /// <returns>
    ///     <c>IsValid</c> and the joined error message, or <see cref="string.Empty" /> if validation passed.
    /// </returns>
    public static async Task<(bool IsValid, string ErrorMessage)> ValidateWithMessageAsync<T>(
        this IValidator<T> validator, T value, CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(value, cancellationToken);
        if (result.IsValid) return (true, string.Empty);

        var errorMessage = string.Join(", ", result.Errors.Select(e => e.ErrorMessage));
        return (false, errorMessage);
    }
}
