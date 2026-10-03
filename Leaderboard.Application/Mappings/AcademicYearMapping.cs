using AutoMapper;
using Leaderboard.Domain.Dtos.AcademicYear;
using Leaderboard.Domain.Entities;

namespace Leaderboard.Application.Mappings;

public class AcademicYearMapping : Profile
{
    public AcademicYearMapping()
    {
        CreateMap<AcademicYear, AcademicYearDto>().ReverseMap();
    }
}