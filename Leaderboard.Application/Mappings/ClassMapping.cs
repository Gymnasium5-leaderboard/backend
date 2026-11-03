using AutoMapper;
using Leaderboard.Domain.Dtos.Class;
using Leaderboard.Domain.Entities;

namespace Leaderboard.Application.Mappings;

public class ClassMapping : Profile
{
    public ClassMapping()
    {
        CreateMap<SchoolClass, ClassDto>()
            .ForCtorParam(nameof(ClassDto.StudentCount), opt => opt.MapFrom(_ => 0))
            .ForCtorParam(nameof(ClassDto.Score), opt => opt.MapFrom(_ => 0d))
            .ReverseMap();
        CreateMap<SchoolClass, CreateClassDto>().ReverseMap();
        CreateMap<SchoolClass, UpdateClassDto>().ReverseMap();
    }
}