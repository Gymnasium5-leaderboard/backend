using AutoMapper;
using Leaderboard.Domain.Dtos.Class;
using Leaderboard.Domain.Entities;

namespace Leaderboard.Application.Mappings;

public class ClassMapping : Profile
{
    public ClassMapping()
    {
        CreateMap<SchoolClass, ClassDto>().ReverseMap();
        CreateMap<SchoolClass, CreateClassDto>().ReverseMap();
        CreateMap<SchoolClass, UpdateClassDto>().ReverseMap();
    }
}
