using Leaderboard.Domain.Dtos.Owner;
using Leaderboard.Domain.Results;

namespace Leaderboard.Domain.Interfaces.Service;

public interface IOwnerService
{
    Task<BaseResult<OwnerDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    
    Task<BaseResult<OwnerDto>> CreateAsync(CreateOwnerDto dto, CancellationToken cancellationToken = default);

    Task<BaseResult<OwnerDto>> UpdateAsync(long ownerId, UpdateOwnerDto dto,
        CancellationToken cancellationToken = default);
}
