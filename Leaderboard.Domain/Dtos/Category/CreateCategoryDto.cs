using Leaderboard.Domain.Interfaces.Validation;

namespace Leaderboard.Domain.Dtos.Category;

public record CreateCategoryDto(string Name) : IValidatableCategory;
