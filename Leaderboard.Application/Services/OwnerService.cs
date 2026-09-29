using AutoMapper;
using FluentValidation;
using Leaderboard.Application.Enums;
using Leaderboard.Application.Extensions;
using Leaderboard.Application.Mappings;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Owner;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Repository;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Interfaces.Validation;
using Leaderboard.Domain.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Application.Services;

public class OwnerService(
    IBaseRepository<LeaderboardOwner> ownerRepository,
    IPasswordHasher<LeaderboardOwner> passwordHasher,
    IValidator<CreateOwnerDto> createValidator,
    IValidator<IValidatableName> nameValidator,
    IMapper mapper) : IOwnerService
{
    public async Task<BaseResult<OwnerDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var owner = await ownerRepository.GetAll().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return owner == null
            ? BaseResult<OwnerDto>.Failure(ErrorMessage.OwnerNotFound, (int)ErrorCodes.OwnerNotFound)
            : BaseResult<OwnerDto>.Success(mapper.Map<OwnerDto>(owner));
    }

    public async Task<BaseResult<OwnerDto>> CreateAsync(CreateOwnerDto dto,
        CancellationToken cancellationToken = default)
    {
        var (isValid, errorMessage) = await createValidator.ValidateWithMessageAsync(dto, cancellationToken);
        if (!isValid) return BaseResult<OwnerDto>.Failure(errorMessage, (int)ErrorCodes.InvalidProperty);

        if (await ownerRepository.GetAll().AnyAsync(x => x.Login == dto.Login, cancellationToken))
            return BaseResult<OwnerDto>.Failure(ErrorMessage.OwnerAlreadyExists, (int)ErrorCodes.OwnerAlreadyExists);

        var owner = mapper.Map<LeaderboardOwner>(dto);
        owner.PasswordHash = passwordHasher.HashPassword(owner, dto.Password);

        await ownerRepository.CreateAsync(owner, cancellationToken);
        await ownerRepository.SaveChangesAsync(cancellationToken);

        return BaseResult<OwnerDto>.Success(mapper.Map<OwnerDto>(owner));
    }

    public async Task<BaseResult<OwnerDto>> UpdateAsync(long ownerId, UpdateOwnerDto dto,
        CancellationToken cancellationToken = default)
    {
        var (isValid, errorMessage) = await nameValidator.ValidateWithMessageAsync(dto, cancellationToken);
        if (!isValid) return BaseResult<OwnerDto>.Failure(errorMessage, (int)ErrorCodes.InvalidProperty);

        var owner = await ownerRepository.GetAll().FirstOrDefaultAsync(x => x.Id == ownerId, cancellationToken);
        if (owner == null)
            return BaseResult<OwnerDto>.Failure(ErrorMessage.OwnerNotFound, (int)ErrorCodes.OwnerNotFound);

        mapper.Map(dto, owner);
        await ownerRepository.SaveChangesAsync(cancellationToken);

        return BaseResult<OwnerDto>.Success(mapper.Map<OwnerDto>(owner));
    }
}