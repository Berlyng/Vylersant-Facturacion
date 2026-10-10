using Microsoft.Extensions.Options;
using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Application.Users;
using Vylersant_Facturacion.Domain.Authentication;

namespace Vylersant_Facturacion.Application.Authentication;

public sealed class RefreshSessionService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RefreshTokenOptions _options;

    public RefreshSessionService(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IRefreshTokenService refreshTokenService,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IOptions<RefreshTokenOptions> options)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _refreshTokenService = refreshTokenService;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    public async Task<RefreshSessionResult> ExecuteAsync(
        RefreshSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new InvalidRefreshTokenException();
        }

        var tokenHash =
            _refreshTokenService.Hash(request.RefreshToken);

        var currentRefreshToken =
            await _refreshTokenRepository.GetByHashAsync(
                tokenHash,
                cancellationToken);

        var now = DateTime.UtcNow;

        if (currentRefreshToken is null ||
            !currentRefreshToken.IsActive(now))
        {
            throw new InvalidRefreshTokenException();
        }

        var user =
            await _userRepository.GetByIdAsync(
                currentRefreshToken.UserId,
                cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new InvalidRefreshTokenException();
        }

        // Rotamos el refresh token anterior.
        currentRefreshToken.Revoke(now);

        var newRefreshTokenValue =
            _refreshTokenService.Generate();

        var newRefreshTokenHash =
            _refreshTokenService.Hash(
                newRefreshTokenValue);

        var newRefreshTokenExpiresAtUtc =
            now.AddDays(_options.ExpirationDays);

        var newRefreshToken = new RefreshToken(
            user.Id,
            newRefreshTokenHash,
            now,
            newRefreshTokenExpiresAtUtc);

        await _refreshTokenRepository.AddAsync(
            newRefreshToken,
            cancellationToken);

        var accessToken =
            _tokenService.Generate(user);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new RefreshSessionResult(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            newRefreshTokenValue,
            newRefreshTokenExpiresAtUtc);
    }
}