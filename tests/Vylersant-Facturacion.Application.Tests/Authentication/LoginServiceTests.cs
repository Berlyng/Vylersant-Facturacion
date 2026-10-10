using Microsoft.Extensions.Options;
using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Authentication;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Application.Users;
using Vylersant_Facturacion.Domain.Authentication;
using Vylersant_Facturacion.Domain.Entities.Users;

namespace Vylersant_Facturacion.Application.Tests.Authentication;

public class LoginServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnUser_WhenCredentialsAreValid()
    {
        // Arrange
        const string rawPassword = "Password123!";

        var user = new User(
            Guid.NewGuid(),
            "John Doe",
            "test@example.com",
            $"HASHED:{rawPassword}",
            UserRole.Owner);

        var userRepository = new FakeUserRepository
        {
            ExistingUser = user
        };

        var passwordHasher = new FakePasswordHasher();
        var tokenService = new FakeTokenService();
        var refreshTokenService = new FakeRefreshTokenService();
        var refreshTokenRepository = new FakeRefreshTokenRepository();
        var unitOfWork = new FakeUnitOfWork();

        var refreshOptions = Options.Create(
            new RefreshTokenOptions
            {
                ExpirationDays = 7
            });

        var service = new LoginService(
            userRepository,
            passwordHasher,
            tokenService,
            refreshTokenService,
            refreshTokenRepository,
            unitOfWork,
            refreshOptions);

        var request = new LoginRequest(
            "test@example.com",
            rawPassword);

        // Act
        var result = await service.ExecuteAsync(
            request,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(user.BusinessId, result.BusinessId);
        Assert.Equal(user.Name, result.Name);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(UserRole.Owner, result.Role);

        // Access Token
        Assert.Equal(
            "fake-access-token",
            result.AccessToken);

        Assert.True(
            result.AccessTokenExpiresAtUtc > DateTime.UtcNow);

        // Refresh Token original
        Assert.Equal(
            "fake-refresh-token",
            result.RefreshToken);

        Assert.True(
            result.RefreshTokenExpiresAtUtc > DateTime.UtcNow);

        // El token persistido debe ser el HASH,
        // no el refresh token original.
        Assert.NotNull(
            refreshTokenRepository.AddedToken);

        Assert.Equal(
            user.Id,
            refreshTokenRepository.AddedToken.UserId);

        Assert.Equal(
            "HASH:fake-refresh-token",
            refreshTokenRepository.AddedToken.TokenHash);

        Assert.NotEqual(
            result.RefreshToken,
            refreshTokenRepository.AddedToken.TokenHash);

        Assert.True(
            refreshTokenRepository.AddedToken.IsActive(
                DateTime.UtcNow));

        Assert.Equal(
            1,
            refreshTokenService.GenerateCallCount);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenEmailDoesNotExist()
    {
        // Arrange
        var userRepository = new FakeUserRepository
        {
            ExistingUser = null
        };

        var passwordHasher = new FakePasswordHasher();
        var tokenService = new FakeTokenService();
        var refreshTokenService = new FakeRefreshTokenService();
        var refreshTokenRepository = new FakeRefreshTokenRepository();
        var unitOfWork = new FakeUnitOfWork();

        var refreshOptions = CreateRefreshTokenOptions();

        var service = new LoginService(
            userRepository,
            passwordHasher,
            tokenService,
            refreshTokenService,
            refreshTokenRepository,
            unitOfWork,
            refreshOptions);

        var request = new LoginRequest(
            "nonexistent@example.com",
            "Password123!");

        // Act
        var action = () =>
            service.ExecuteAsync(
                request,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            action);

        Assert.Equal(
            0,
            refreshTokenService.GenerateCallCount);

        Assert.Null(
            refreshTokenRepository.AddedToken);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenPasswordIsIncorrect()
    {
        // Arrange
        const string correctPassword = "PasswordCorrecto123!";

        var user = new User(
            Guid.NewGuid(),
            "John Doe",
            "test@example.com",
            $"HASHED:{correctPassword}",
            UserRole.Owner);

        var userRepository = new FakeUserRepository
        {
            ExistingUser = user
        };

        var passwordHasher = new FakePasswordHasher();
        var tokenService = new FakeTokenService();
        var refreshTokenService = new FakeRefreshTokenService();
        var refreshTokenRepository = new FakeRefreshTokenRepository();
        var unitOfWork = new FakeUnitOfWork();

        var refreshOptions = CreateRefreshTokenOptions();

        var service = new LoginService(
            userRepository,
            passwordHasher,
            tokenService,
            refreshTokenService,
            refreshTokenRepository,
            unitOfWork,
            refreshOptions);

        var request = new LoginRequest(
            "test@example.com",
            "PasswordIncorrecto123!");

        // Act
        var action = () =>
            service.ExecuteAsync(
                request,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            action);

        // MUY IMPORTANTE:
        // si la contraseña es incorrecta,
        // no debe crearse ninguna sesión.
        Assert.Equal(
            0,
            refreshTokenService.GenerateCallCount);

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
        const string rawPassword = "Password123!";

        var user = new User(
            Guid.NewGuid(),
            "John Doe",
            "test@example.com",
            $"HASHED:{rawPassword}",
            UserRole.Owner);

        user.Deactivate();

        var userRepository = new FakeUserRepository
        {
            ExistingUser = user
        };

        var passwordHasher = new FakePasswordHasher();
        var tokenService = new FakeTokenService();
        var refreshTokenService = new FakeRefreshTokenService();
        var refreshTokenRepository = new FakeRefreshTokenRepository();
        var unitOfWork = new FakeUnitOfWork();

        var refreshOptions = CreateRefreshTokenOptions();

        var service = new LoginService(
            userRepository,
            passwordHasher,
            tokenService,
            refreshTokenService,
            refreshTokenRepository,
            unitOfWork,
            refreshOptions);

        var request = new LoginRequest(
            "test@example.com",
            rawPassword);

        // Act
        var action = () =>
            service.ExecuteAsync(
                request,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InactiveUserException>(
            action);

        Assert.Equal(
            0,
            refreshTokenService.GenerateCallCount);

        Assert.Null(
            refreshTokenRepository.AddedToken);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAssociateRefreshTokenWithAuthenticatedUser()
    {
        // Arrange
        const string rawPassword = "Password123!";

        var user = new User(
            Guid.NewGuid(),
            "John Doe",
            "test@example.com",
            $"HASHED:{rawPassword}",
            UserRole.Owner);

        var userRepository = new FakeUserRepository
        {
            ExistingUser = user
        };

        var passwordHasher = new FakePasswordHasher();
        var tokenService = new FakeTokenService();
        var refreshTokenService = new FakeRefreshTokenService();
        var refreshTokenRepository = new FakeRefreshTokenRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new LoginService(
            userRepository,
            passwordHasher,
            tokenService,
            refreshTokenService,
            refreshTokenRepository,
            unitOfWork,
            CreateRefreshTokenOptions());

        var request = new LoginRequest(
            user.Email,
            rawPassword);

        // Act
        await service.ExecuteAsync(
            request,
            CancellationToken.None);

        // Assert
        Assert.NotNull(
            refreshTokenRepository.AddedToken);

        Assert.Equal(
            user.Id,
            refreshTokenRepository.AddedToken.UserId);
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

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? ExistingUser { get; set; }

        public User? AddedUser { get; private set; }

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExistingUser);
        }

        public Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            AddedUser = user;

            return Task.CompletedTask;
        }

        public Task<User> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(ExistingUser);
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            return $"HASHED:{password}";
        }

        public bool Verify(
            string password,
            string passwordHash)
        {
            return passwordHash == $"HASHED:{password}";
        }
    }

    private sealed class FakeTokenService : ITokenService
    {
        public AccessToken Generate(User user)
        {
            return new AccessToken(
                "fake-access-token",
                DateTime.UtcNow.AddMinutes(30));
        }
    }

    private sealed class FakeRefreshTokenService
        : IRefreshTokenService
    {
        public int GenerateCallCount { get; private set; }

        public string Generate()
        {
            GenerateCallCount++;

            return "fake-refresh-token";
        }

        public string Hash(string token)
        {
            return $"HASH:{token}";
        }
    }

    private sealed class FakeRefreshTokenRepository
        : IRefreshTokenRepository
    {
        public RefreshToken? AddedToken { get; private set; }

        public Task<RefreshToken?> GetByHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<RefreshToken?>(null);
        }

        public Task AddAsync(
            RefreshToken refreshToken,
            CancellationToken cancellationToken = default)
        {
            AddedToken = refreshToken;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;

            return Task.FromResult(1);
        }
    }
}