using AutoMapper;
using Leaderboard.Domain.Dtos.Student;
using Leaderboard.Domain.Entities;

namespace Leaderboard.Application.Mappings;

public class StudentMapping : Profile
{
    public StudentMapping()
    {
        CreateMap<Student, StudentDto>()
            .ForCtorParam(nameof(StudentDto.ClassName), o => o.MapFrom(s => s.Class.DisplayName))
            .ReverseMap();
        CreateMap<Student, CreateStudentDto>().ReverseMap();
        CreateMap<Student, UpdateStudentDto>().ReverseMap();
    }
}
