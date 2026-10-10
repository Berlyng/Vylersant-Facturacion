using Microsoft.Extensions.Options;
using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Authentication;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Application.Users;
using Vylersant_Facturacion.Domain.Authentication;
using Vylersant_Facturacion.Domain.Entities.Users;

namespace Vylersant_Facturacion.Application.Tests.Authentication;

public class RefreshSessionServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldRenewToken_WhenTokenIsValid()
    {
        // Arrange
        var user = CreateUser();

        var oldToken = new RefreshToken(
            user.Id,
            "HASH:old-refresh-token",
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(6));

        var refreshTokenRepository = new FakeRefreshTokenRepository
        {
            ExistingToken = oldToken
        };

        var userRepository = new FakeUserRepository
        {
            ExistingUser = user
        };

        var refreshTokenService = new FakeRefreshTokenService();

        var tokenService = new FakeTokenService();

        var unitOfWork = new FakeUnitOfWork();

        var service = new RefreshSessionService(
            refreshTokenRepository,
            userRepository,
            refreshTokenService,
            tokenService,
            unitOfWork,
            CreateRefreshTokenOptions());

        var request = new RefreshSessionRequest(
            "old-refresh-token");

        // Act
        var result = await service.ExecuteAsync(
            request,
            CancellationToken.None);

        // Assert

        // El token anterior debe quedar revocado.
        Assert.False(
            oldToken.IsActive(DateTime.UtcNow));

        Assert.NotNull(
            oldToken.RevokedAtUtc);

        // Debe haberse agregado un nuevo refresh token.
        Assert.NotNull(
            refreshTokenRepository.AddedToken);

        Assert.Equal(
            user.Id,
            refreshTokenRepository.AddedToken.UserId);

        Assert.Equal(
            "HASH:new-refresh-token",
            refreshTokenRepository.AddedToken.TokenHash);

        Assert.True(
            refreshTokenRepository.AddedToken.IsActive(
                DateTime.UtcNow));

        // El cliente recibe el token original, no el hash.
        Assert.Equal(
            "new-refresh-token",
            result.RefreshToken);

        Assert.NotEqual(
            refreshTokenRepository.AddedToken.TokenHash,
            result.RefreshToken);

        // Debe generar también un nuevo Access Token.
        Assert.Equal(
            "fake-access-token",
            result.AccessToken);

        Assert.True(
            result.AccessTokenExpiresAtUtc > DateTime.UtcNow);

        Assert.True(
            result.RefreshTokenExpiresAtUtc > DateTime.UtcNow);

        // Persistimos la revocación y el nuevo refresh token juntos.
        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenTokenDoesNotExist()
    {
        // Arrange
        var refreshTokenRepository = new FakeRefreshTokenRepository
        {
            ExistingToken = null
        };

        var userRepository = new FakeUserRepository();

        var refreshTokenService = new FakeRefreshTokenService();

        var tokenService = new FakeTokenService();

        var unitOfWork = new FakeUnitOfWork();

        var service = new RefreshSessionService(
            refreshTokenRepository,
            userRepository,
            refreshTokenService,
            tokenService,
            unitOfWork,
            CreateRefreshTokenOptions());

        var request = new RefreshSessionRequest(
            "non-existent-token");

        // Act
        var action = () =>
            service.ExecuteAsync(
                request,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(
            action);

        Assert.Null(
            refreshTokenRepository.AddedToken);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenTokenIsRevoked()
    {
        // Arrange
        var user = CreateUser();

        var oldToken = new RefreshToken(
            user.Id,
            "HASH:old-refresh-token",
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(6));

        oldToken.Revoke(DateTime.UtcNow);

        var refreshTokenRepository = new FakeRefreshTokenRepository
        {
            ExistingToken = oldToken
        };

        var userRepository = new FakeUserRepository
        {
            ExistingUser = user
        };

        var refreshTokenService = new FakeRefreshTokenService();

        var tokenService = new FakeTokenService();

        var unitOfWork = new FakeUnitOfWork();

        var service = new RefreshSessionService(
            refreshTokenRepository,
            userRepository,
            refreshTokenService,
            tokenService,
            unitOfWork,
            CreateRefreshTokenOptions());

        var request = new RefreshSessionRequest(
            "old-refresh-token");

        // Act
        var action = () =>
            service.ExecuteAsync(
                request,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(
            action);

        Assert.Null(
            refreshTokenRepository.AddedToken);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenTokenIsExpired()
    {
        // Arrange
        var user = CreateUser();

        var oldToken = new RefreshToken(
            user.Id,
            "HASH:old-refresh-token",
            DateTime.UtcNow.AddDays(-5),
            DateTime.UtcNow.AddDays(-1));

        var refreshTokenRepository = new FakeRefreshTokenRepository
        {
            ExistingToken = oldToken
        };

        var userRepository = new FakeUserRepository
        {
            ExistingUser = user
        };

        var refreshTokenService = new FakeRefreshTokenService();

        var tokenService = new FakeTokenService();

        var unitOfWork = new FakeUnitOfWork();

        var service = new RefreshSessionService(
            refreshTokenRepository,
            userRepository,
            refreshTokenService,
            tokenService,
            unitOfWork,
            CreateRefreshTokenOptions());

        var request = new RefreshSessionRequest(
            "old-refresh-token");

        // Act
        var action = () =>
            service.ExecuteAsync(
                request,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(
            action);

        Assert.Null(
            refreshTokenRepository.AddedToken);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var oldToken = new RefreshToken(
            userId,
            "HASH:old-refresh-token",
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(6));

        var refreshTokenRepository = new FakeRefreshTokenRepository
        {
            ExistingToken = oldToken
        };

        var userRepository = new FakeUserRepository
        {
            ExistingUser = null
        };

        var refreshTokenService = new FakeRefreshTokenService();

        var tokenService = new FakeTokenService();

        var unitOfWork = new FakeUnitOfWork();

        var service = new RefreshSessionService(
            refreshTokenRepository,
            userRepository,
            refreshTokenService,
            tokenService,
            unitOfWork,
            CreateRefreshTokenOptions());

        var request = new RefreshSessionRequest(
            "old-refresh-token");

        // Act
        var action = () =>
            service.ExecuteAsync(
                request,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(
            action);

        Assert.True(
            oldToken.IsActive(DateTime.UtcNow));

        Assert.Null(
            refreshTokenRepository.AddedToken);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenUserIsInactive()
    {
        // Arrange
        var user = CreateUser();

        user.Deactivate();

        var oldToken = new RefreshToken(
            user.Id,
            "HASH:old-refresh-token",
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(6));

        var refreshTokenRepository = new FakeRefreshTokenRepository
        {
            ExistingToken = oldToken
        };

        var userRepository = new FakeUserRepository
        {
            ExistingUser = user
        };

        var refreshTokenService = new FakeRefreshTokenService();

        var tokenService = new FakeTokenService();

        var unitOfWork = new FakeUnitOfWork();

        var service = new RefreshSessionService(
            refreshTokenRepository,
            userRepository,
            refreshTokenService,
            tokenService,
            unitOfWork,
            CreateRefreshTokenOptions());

        var request = new RefreshSessionRequest(
            "old-refresh-token");

        // Act
        var action = () =>
            service.ExecuteAsync(
                request,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(
            action);

        Assert.True(
            oldToken.IsActive(DateTime.UtcNow));

        Assert.Null(
            refreshTokenRepository.AddedToken);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenRefreshTokenIsEmpty()
    {
        // Arrange
        var refreshTokenRepository =
            new FakeRefreshTokenRepository();

        var userRepository =
            new FakeUserRepository();

        var refreshTokenService =
            new FakeRefreshTokenService();

        var tokenService =
            new FakeTokenService();

        var unitOfWork =
            new FakeUnitOfWork();

        var service = new RefreshSessionService(
            refreshTokenRepository,
            userRepository,
            refreshTokenService,
            tokenService,
            unitOfWork,
            CreateRefreshTokenOptions());

        var request = new RefreshSessionRequest("");

        // Act
        var action = () =>
            service.ExecuteAsync(
                request,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(
            action);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    private static User CreateUser()
    {
        return new User(
            Guid.NewGuid(),
            "John Doe",
            "test@example.com",
            "hashed-password",
            UserRole.Owner);
    }

    private static IOptions<RefreshTokenOptions>
        CreateRefreshTokenOptions()
    {
        return Options.Create(
            new RefreshTokenOptions
            {
                ExpirationDays = 7
            });
    }

    #region Fakes

    private sealed class FakeRefreshTokenRepository
        : IRefreshTokenRepository
    {
        public RefreshToken? ExistingToken { get; set; }

        public RefreshToken? AddedToken { get; private set; }

        public Task<RefreshToken?> GetByHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                ExistingToken);
        }

        public Task AddAsync(
            RefreshToken refreshToken,
            CancellationToken cancellationToken = default)
        {
            AddedToken = refreshToken;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeRefreshTokenService
        : IRefreshTokenService
    {
        public int GenerateCallCount { get; private set; }

        public string Generate()
        {
            GenerateCallCount++;

            return "new-refresh-token";
        }

        public string Hash(string token)
        {
            return $"HASH:{token}";
        }
    }

    private sealed class FakeTokenService
        : ITokenService
    {
        public int GenerateCallCount { get; private set; }

        public AccessToken Generate(User user)
        {
            GenerateCallCount++;

            return new AccessToken(
                "fake-access-token",
                DateTime.UtcNow.AddMinutes(30));
        }
    }

    private sealed class FakeUnitOfWork
        : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;

            return Task.FromResult(1);
        }
    }

    private sealed class FakeUserRepository
        : IUserRepository
    {
        public User? ExistingUser { get; set; }

        public User? AddedUser { get; private set; }

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                ExistingUser);
        }

        public Task<User?> GetByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (ExistingUser?.Id != userId)
            {
                return Task.FromResult<User?>(
                    null);
            }

            return Task.FromResult<User?>(
                ExistingUser);
        }

        public Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            AddedUser = user;

            return Task.CompletedTask;
        }
    }

    #endregion
}