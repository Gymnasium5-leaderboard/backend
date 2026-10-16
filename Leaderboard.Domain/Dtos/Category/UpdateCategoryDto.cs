using Leaderboard.Domain.Interfaces.Validation;

namespace Leaderboard.Domain.Dtos.Category;

public record UpdateCategoryDto(string Name) : IValidatableCategory;
