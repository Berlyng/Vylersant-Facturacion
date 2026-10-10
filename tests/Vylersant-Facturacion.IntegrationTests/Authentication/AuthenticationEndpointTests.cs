using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Authentication;
using Vylersant_Facturacion.Application.Bussinesses;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Application.Users;
using Vylersant_Facturacion.Contracts.Authentication;
using Vylersant_Facturacion.Domain.Authentication;
using Vylersant_Facturacion.Domain.Entities.Businesses;
using Vylersant_Facturacion.Domain.Entities.Users;

namespace Vylersant_Facturacion.IntegrationTests.Authentication
{
    public class AuthenticationEndpointTests : IClassFixture<Vylersant_FacturacionApiFactory>
    {

        private WebApplicationFactory<Program> CreateSessionFactory(
    User user)
        {
            var userRepository =
                new FakeUserRepository(user);

            var refreshTokenRepository =
                new InMemoryRefreshTokenRepository();

            var unitOfWork =
                new FakeUnitOfWork();

            return _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IUserRepository>();
                    services.RemoveAll<IRefreshTokenRepository>();
                    services.RemoveAll<IPasswordHasher>();
                    services.RemoveAll<IUnitOfWork>();

                    services.AddSingleton<IUserRepository>(
                        userRepository);

                    services.AddSingleton<IRefreshTokenRepository>(
                        refreshTokenRepository);

                    services.AddSingleton<IPasswordHasher>(
                        new FakePasswordHasher());

                    services.AddSingleton<IUnitOfWork>(
                        unitOfWork);
                });
            });
        }
        private readonly Vylersant_FacturacionApiFactory _factory;
        private readonly HttpClient _client;

        public AuthenticationEndpointTests(Vylersant_FacturacionApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Me_ShouldReturnUnauthorized_WhenTokenIsMissing()
        {
            var response = await _client.GetAsync("/api/auth/me");
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Me_ShouldReturnUserInfo_WhenTokenIsValid()
        {
            // Arrange
            var user = new User(
                Guid.NewGuid(),
                "Juan Pérez",
                "juan@email.com",
                "hashed-password",
                UserRole.Owner);

            using var scope = _factory.Services.CreateScope();

            var tokenService =
                scope.ServiceProvider.GetRequiredService<ITokenService>();

            var accessToken =
                tokenService.Generate(user);

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "/api/auth/me");

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken.Token);

            // Act
            var response =
                await _client.SendAsync(request);

            // Assert
            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            var result =
                await response.Content
                    .ReadFromJsonAsync<CurrentUserResponse>();

            Assert.NotNull(result);

            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(user.BusinessId, result.BusinessId);
            Assert.Equal(user.Name, result.Name);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(UserRole.Owner.ToString(), result.Role);
        }
        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsIncorrect()
        {
            // Arrange

            var user = new User(
                Guid.NewGuid(),
                "Juan Pérez",
                "juan@email.com",
                "HASHED:PasswordCorrecto123!",
                UserRole.Owner);

            using var factory = CreateFactory(user);
            using var client = factory.CreateClient();

            var request =
                new Vylersant_Facturacion.Contracts.Authentication.LoginRequest(
                    "juan@email.com",
                    "PasswordIncorrecto123!");

            // Act

            var response =
                await client.PostAsJsonAsync(
                    "/api/auth/login",
                    request);

            // Assert

            var problem =
    await response.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problem);

            Assert.Equal(
                "Credenciales inválidas",
                problem.Title);

            Assert.Equal(
                StatusCodes.Status401Unauthorized,
                problem.Status);

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);


        }

        [Fact]
        public async Task Register_ShouldReturnConflict_WhenEmailAlreadyExists()
        {
            // Arrange

            var existingUser = new User(
                Guid.NewGuid(),
                "Juan Pérez",
                "juan@email.com",
                "HASHED:Password123!",
                UserRole.Owner);

            using var factory = CreateFactory(existingUser);
            using var client = factory.CreateClient();

            var request =
                new Vylersant_Facturacion.Contracts.Authentication.RegisterBusinessRequest(
                    "Negocio XYZ",
                    "Otro Propietario",
                    "juan@email.com",
                    "OtraPassword123!");

            // Act

            var response =
                await client.PostAsJsonAsync(
                    "/api/auth/register",
                    request);

            // Assert

            Assert.Equal(
                HttpStatusCode.Conflict,
                response.StatusCode);

            var problem =
                await response.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problem);

            Assert.Equal(
                "Correo ya existe",
                problem.Title);

            Assert.Equal(
                StatusCodes.Status409Conflict,
                problem.Status);
        }

        [Fact]
        public async Task Refresh_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
        {
            // Arrange
            const string password = "Password123!";

            var user = new User(
                Guid.NewGuid(),
                "John Doe",
                "test@example.com",
                $"HASHED:{password}",
                UserRole.Owner);

            using var factory =
                CreateSessionFactory(user);

            using var client =
                factory.CreateClient();

            var loginRequest =
                new Vylersant_Facturacion.Contracts.Authentication.LoginRequest(
                    user.Email,
                    password);

            // Act - Login
            var loginResponse =
                await client.PostAsJsonAsync(
                    "/api/auth/login",
                    loginRequest);

            // Assert - Login
            Assert.Equal(
                HttpStatusCode.OK,
                loginResponse.StatusCode);

            var loginResult =
                await loginResponse.Content
                    .ReadFromJsonAsync<LoginResponse>();

            Assert.NotNull(loginResult);

            Assert.False(
                string.IsNullOrWhiteSpace(
                    loginResult.AccessToken));

            Assert.False(
                string.IsNullOrWhiteSpace(
                    loginResult.RefreshToken));

            var oldRefreshToken =
                loginResult.RefreshToken;

            // Act - Refresh
            var refreshRequest =
                new Vylersant_Facturacion.Contracts.Authentication.RefreshSessionRequest(
                    oldRefreshToken);

            var refreshResponse =
                await client.PostAsJsonAsync(
                    "/api/auth/refresh",
                    refreshRequest);

            // Assert - Refresh
            Assert.Equal(
                HttpStatusCode.OK,
                refreshResponse.StatusCode);

            var refreshResult =
                await refreshResponse.Content
                    .ReadFromJsonAsync<RefreshSessionResponse>();

            Assert.NotNull(refreshResult);

            Assert.False(
                string.IsNullOrWhiteSpace(
                    refreshResult.AccessToken));

            Assert.False(
                string.IsNullOrWhiteSpace(
                    refreshResult.RefreshToken));

            Assert.NotEqual(
                oldRefreshToken,
                refreshResult.RefreshToken);

            Assert.True(
                refreshResult.AccessTokenExpiresAtUtc >
                DateTime.UtcNow);

            Assert.True(
                refreshResult.RefreshTokenExpiresAtUtc >
                DateTime.UtcNow);
        }

        [Fact]
        public async Task Refresh_ShouldReturnUnauthorized_WhenOldRefreshTokenIsReused()
        {



            // Arrange
            const string password = "Password123!";

            var user = new User(
                Guid.NewGuid(),
                "John Doe",
                "test@example.com",
                $"HASHED:{password}",
                UserRole.Owner);

            using var factory =
                CreateSessionFactory(user);

            using var client =
                factory.CreateClient();

            // Login
            var loginResponse =
                await client.PostAsJsonAsync(
                    "/api/auth/login",
                    new Vylersant_Facturacion.Contracts.Authentication.LoginRequest(
                        user.Email,
                        password));



            Assert.Equal(
                HttpStatusCode.OK,
                loginResponse.StatusCode);

            var loginResult =
                await loginResponse.Content
                    .ReadFromJsonAsync<LoginResponse>();

            Assert.NotNull(loginResult);

            var oldRefreshToken =
                loginResult.RefreshToken;

            // Primera renovación
            var firstRefreshResponse =
                await client.PostAsJsonAsync(
                    "/api/auth/refresh",
                    new Vylersant_Facturacion.Contracts.Authentication.RefreshSessionRequest(
                        oldRefreshToken));

            Assert.Equal(
                HttpStatusCode.OK,
                firstRefreshResponse.StatusCode);

            // Act
            // Intentamos reutilizar el token anterior.


            var secondRefreshResponse =
                await client.PostAsJsonAsync(
                    "/api/auth/refresh",
                    new Vylersant_Facturacion.Contracts.Authentication.RefreshSessionRequest(
                        oldRefreshToken));

            var problem =
                 await secondRefreshResponse.Content
                   .ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problem);

            Assert.Equal(
                "Sesión inválida",
                problem.Title);

            Assert.Equal(
                StatusCodes.Status401Unauthorized,
                problem.Status);

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                secondRefreshResponse.StatusCode);
        }



        private static string CreateAccessToken(
              Guid userId,
              Guid businessId,
              string name,
              string email,
              string role)
        {

            var claims = new[]
            {
                new Claim(TokenClaimNames.UserId, userId.ToString()),
                new Claim(TokenClaimNames.BusinessId, businessId.ToString()),
                new Claim(TokenClaimNames.Email, email),
                new Claim(TokenClaimNames.Name, name),
                new Claim(TokenClaimNames.Role, role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    Vylersant_FacturacionApiFactory.Key));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Vylersant_FacturacionApiFactory.JwtIssuer,
                audience: Vylersant_FacturacionApiFactory.JwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }


        private WebApplicationFactory<Program> CreateFactory(
    User? existingUser)
        {
            return _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IUserRepository>();
                    services.RemoveAll<IBussinesRepository>();
                    services.RemoveAll<IPasswordHasher>();
                    services.RemoveAll<IUnitOfWork>();

                    services.AddSingleton<IUserRepository>(
                        new FakeUserRepository(existingUser));

                    services.AddSingleton<IBussinesRepository>(
                        new FakeBusinessRepository());

                    services.AddSingleton<IPasswordHasher>(
                        new FakePasswordHasher());

                    services.AddSingleton<IUnitOfWork>(
                        new FakeUnitOfWork());
                });
            });
        }



        private sealed class FakeUserRepository : IUserRepository
        {
            private readonly User? _existingUser;

            public FakeUserRepository(User? existingUser)
            {
                _existingUser = existingUser;
            }

            public Task<User?> GetByEmailAsync(
                string email,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_existingUser);
            }

            public Task AddAsync(
                User user,
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task<User> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
            {

                User? result =_existingUser.Id == userId
                    ? _existingUser
              : null;
                return Task.FromResult(result!);
            }
        }

        private sealed class FakeBusinessRepository
    : IBussinesRepository
        {
            public Task AddAsync(
                Business business,
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }

        private sealed class FakePasswordHasher
    : IPasswordHasher
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

        private sealed class InMemoryRefreshTokenRepository
    : IRefreshTokenRepository
        {
            private readonly List<RefreshToken> _tokens = [];

            public Task<RefreshToken?> GetByHashAsync(
                string tokenHash,
                CancellationToken cancellationToken = default)
            {
                var token = _tokens
                    .FirstOrDefault(x => x.TokenHash == tokenHash);

                return Task.FromResult(token);
            }

            public Task AddAsync(
                RefreshToken refreshToken,
                CancellationToken cancellationToken = default)
            {
                _tokens.Add(refreshToken);

                return Task.CompletedTask;
            }
        }

     


    }
}
