using Leaderboard.Domain.Dtos.Owner;
using Leaderboard.Domain.Results;

namespace Leaderboard.Domain.Interfaces.Service;

public interface IOwnerService
{
    Task<BaseResult<OwnerDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     Creates an owner after checking the current password of the creator.
    /// </summary>
    Task<BaseResult<OwnerDto>> CreateAsync(long creatorId, CreateOwnerDto dto,
        CancellationToken cancellationToken = default);

    Task<BaseResult<OwnerDto>> UpdateAsync(long ownerId, UpdateOwnerDto dto,
        CancellationToken cancellationToken = default);
}
