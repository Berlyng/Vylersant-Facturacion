using Microsoft.Extensions.Options;
using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Application.Users;
using Vylersant_Facturacion.Domain.Authentication;

namespace Vylersant_Facturacion.Application.Authentication
{
    public sealed class LoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RefreshTokenOptions _refreshTokenOptions;

        public LoginService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService, IRefreshTokenService refreshTokenService, IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork, IOptions<RefreshTokenOptions> refreshTokenOptions)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _refreshTokenOptions = refreshTokenOptions.Value;
        }

        public async Task<LoginResult> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null)
            {
                throw new InvalidCredentialsException();
            }
            if (!user.IsActive)
            {
                throw new InactiveUserException();
            }
            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new InvalidCredentialsException();
            }

            var accessToken = _tokenService.Generate(user);

            var refreshTokenValue =
    _refreshTokenService.Generate();

            var refreshTokenHash =
                _refreshTokenService.Hash(refreshTokenValue);

            var createdAtUtc = DateTime.UtcNow;

            var refreshTokenExpiresAtUtc =
                createdAtUtc.AddDays(
                    _refreshTokenOptions.ExpirationDays);

            var refreshToken = new RefreshToken(
                user.Id,
                refreshTokenHash,
                createdAtUtc,
                refreshTokenExpiresAtUtc);

            await _refreshTokenRepository.AddAsync(
                refreshToken,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return new LoginResult(
                user.Id,
                user.BusinessId,
                user.Name,
                user.Email,
                user.Role,
                accessToken.Token,
                accessToken.ExpiresAtUtc,
                refreshTokenValue,
                refreshTokenExpiresAtUtc);
        }
    }
}
