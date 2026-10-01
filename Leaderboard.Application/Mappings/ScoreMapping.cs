using AutoMapper;
using Leaderboard.Domain.Dtos.Score;
using Leaderboard.Domain.Entities;

namespace Leaderboard.Application.Mappings;

public class ScoreMapping : Profile
{
    public ScoreMapping()
    {
        // Student* and Owner* are flattened from the navigation properties
        CreateMap<ScoreTransaction, ScoreTransactionDto>().ReverseMap();
    }
}