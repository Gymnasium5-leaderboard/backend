using AutoMapper;
using Leaderboard.Domain.Dtos.Owner;
using Leaderboard.Domain.Entities;

namespace Leaderboard.Application.Mappings;

public class OwnerMapping : Profile
{
    public OwnerMapping()
    {
        CreateMap<LeaderboardOwner, OwnerDto>().ReverseMap();
        CreateMap<LeaderboardOwner, CreateOwnerDto>().ReverseMap();
        CreateMap<LeaderboardOwner, UpdateOwnerDto>().ReverseMap();
    }
}