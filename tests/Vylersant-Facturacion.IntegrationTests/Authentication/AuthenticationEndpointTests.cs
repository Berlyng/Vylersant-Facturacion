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
using Vylersant_Facturacion.Application.Bussinesses;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Application.Users;
using Vylersant_Facturacion.Contracts.Authentication;
using Vylersant_Facturacion.Domain.Entities.Businesses;
using Vylersant_Facturacion.Domain.Entities.Users;

namespace Vylersant_Facturacion.IntegrationTests.Authentication
{
    public class AuthenticationEndpointTests : IClassFixture<Vylersant_FacturacionApiFactory>
    {
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
            public Task<int> SaveChangesAsync(
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(1);
            }
        }
    }
}
