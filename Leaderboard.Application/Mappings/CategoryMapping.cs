using AutoMapper;
using Leaderboard.Domain.Dtos.Category;
using Leaderboard.Domain.Entities;

namespace Leaderboard.Application.Mappings;

public class CategoryMapping : Profile
{
    public CategoryMapping()
    {
        CreateMap<Category, CategoryDto>().ReverseMap();
        CreateMap<Category, CreateCategoryDto>().ReverseMap();
        CreateMap<Category, UpdateCategoryDto>().ReverseMap();
    }
}
