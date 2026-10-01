using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using Leaderboard.Application.Enums;
using Leaderboard.Application.Extensions;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Auth;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Provider;
using Leaderboard.Domain.Interfaces.Repository;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Interfaces.Validation;
using Leaderboard.Domain.Results;
using Leaderboard.Domain.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Leaderboard.Application.Services;

public class AuthService(
    IBaseRepository<RefreshToken> refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ITokenProvider tokenProvider,
    IPasswordHasher<LeaderboardOwner> passwordHasher,
    IValidator<IValidatableOwnerPassword> passwordValidator,
    IOptions<JwtSettings> jwtSettings) : IAuthService
{
    private const int RefreshTokenBytes = 32;

    public async Task<BaseResult<TokenDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var owner = await unitOfWork.Owners.GetAll()
            .FirstOrDefaultAsync(x => x.Login == dto.Login, cancellationToken);
        if (owner == null) return InvalidCredentials();

        var verification = passwordHasher.VerifyHashedPassword(owner, owner.PasswordHash, dto.Password);
        if (verification == PasswordVerificationResult.Failed)
            return InvalidCredentials();

        // The hash was made with older parameters: store a fresh one
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            owner.PasswordHash = passwordHasher.HashPassword(owner, dto.Password);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var tokenDto = await IssueTokensAsync(owner, cancellationToken);

        return BaseResult<TokenDto>.Success(tokenDto);
    }

    public async Task<BaseResult<TokenDto>> RefreshAsync(string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(refreshToken);
        var now = DateTime.UtcNow;

        var token = await refreshTokenRepository.GetAll()
            .Include(x => x.Owner)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
        if (token is not { RevokedAt: null } || token.ExpiresAt <= now) return InvalidRefreshToken();

        // Conditional update: of two parallel refreshes with the same token only one wins
        var revoked = await refreshTokenRepository.GetAll()
            .Where(x => x.Id == token.Id && x.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, now), cancellationToken);
        if (revoked == 0) return InvalidRefreshToken();

        var tokenDto = await IssueTokensAsync(token.Owner, cancellationToken);

        return BaseResult<TokenDto>.Success(tokenDto);
    }

    public async Task<BaseResult> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(refreshToken);

        await refreshTokenRepository.GetAll()
            .Where(x => x.TokenHash == tokenHash && x.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, DateTime.UtcNow), cancellationToken);

        return BaseResult.Success();
    }

    public async Task<BaseResult> ChangePasswordAsync(long ownerId, ChangePasswordDto dto,
        CancellationToken cancellationToken = default)
    {
        var (isValid, errorMessage) = await passwordValidator.ValidateWithMessageAsync(dto, cancellationToken);
        if (!isValid) return BaseResult.Failure(errorMessage, (int)ErrorCodes.InvalidProperty);

        var owner = await unitOfWork.Owners.GetAll()
            .FirstOrDefaultAsync(x => x.Id == ownerId, cancellationToken);
        if (owner == null) return BaseResult.Failure(ErrorMessage.OwnerNotFound, (int)ErrorCodes.OwnerNotFound);

        if (passwordHasher.VerifyHashedPassword(owner, owner.PasswordHash, dto.CurrentPassword) ==
            PasswordVerificationResult.Failed)
            return BaseResult.Failure(ErrorMessage.WrongCurrentPassword, (int)ErrorCodes.WrongCurrentPassword);

        // The new password and the revoked sessions are saved together
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        owner.PasswordHash = passwordHasher.HashPassword(owner, dto.Password);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await refreshTokenRepository.GetAll()
            .Where(x => x.OwnerId == ownerId && x.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, DateTime.UtcNow), cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return BaseResult.Success();
    }

    private async Task<TokenDto> IssueTokensAsync(LeaderboardOwner owner, CancellationToken cancellationToken)
    {
        var (accessToken, accessTokenExpiresAt) = tokenProvider.CreateAccessToken(owner);

        // The client gets the raw token, the database keeps only its hash
        var refreshToken = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(RefreshTokenBytes));
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenLifetimeDays);

        await refreshTokenRepository.CreateAsync(new RefreshToken
        {
            OwnerId = owner.Id,
            TokenHash = HashToken(refreshToken),
            ExpiresAt = refreshTokenExpiresAt
        }, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new TokenDto(accessToken, accessTokenExpiresAt, refreshToken, refreshTokenExpiresAt);
    }

    private static string HashToken(string token) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static BaseResult<TokenDto> InvalidCredentials() =>
        BaseResult<TokenDto>.Failure(ErrorMessage.InvalidCredentials, (int)ErrorCodes.InvalidCredentials);

    private static BaseResult<TokenDto> InvalidRefreshToken() =>
        BaseResult<TokenDto>.Failure(ErrorMessage.InvalidRefreshToken, (int)ErrorCodes.InvalidRefreshToken);
}