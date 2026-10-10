using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Vylersant_Facturacion.Domain.Entities.Users;
using Vylersant_Facturacion.Infrastructure.Security;

namespace Vylersant_Facturacion.Infrastructure.Tests.Security
{
    public class JwtTokenServiceTests
    {
        private static JwtTokenService CreateTokenService()
        {
            var settings = new JwtSettings
            {
                Issuer = "Vylersant-Facturacion.Api",
                Audience = "Vylersant-Facturacion.Desktop",

                Key = "EstaEsUnaClaveDePruebaMuyLargaYSeguraParaJwt1234567890!",

                ExpirationMinutes = 30
            };

            return new JwtTokenService(
                Options.Create(settings));
        }
        [Fact]
        public void Generate_ShouldReturnToken()
        {
            // Arrange
            var service = CreateTokenService();

            var user = CreateUser();

            // Act
            var result = service.Generate(user);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
        }

        [Fact]
        public void Generate_ShouldSetFutureExpiration()
        {
            // Arrange
            var service = CreateTokenService();

            var user = CreateUser();

            // Act
            var result = service.Generate(user);

            // Assert
            Assert.True(result.ExpiresAtUtc > DateTime.UtcNow);
        }

        [Fact]
        public void Generate_ShouldContainUserId()
        {
            // Arrange
            var service = CreateTokenService();

            var user = CreateUser();

            // Act
            var result = service.Generate(user);

            var token = ReadToken(result.Token);

            // Assert
            var subject = token.Claims
                .First(x => x.Type == JwtRegisteredClaimNames.Sub)
                .Value;

            Assert.Equal(
                user.Id.ToString(),
                subject);
        }

        [Fact]
        public void Generate_ShouldContainBusinessId()
        {
            // Arrange
            var service = CreateTokenService();

            var user = CreateUser();

            // Act
            var result = service.Generate(user);

            var token = ReadToken(result.Token);

            // Assert
            var businessId = token.Claims
                .First(x => x.Type == "business_id")
                .Value;

            Assert.Equal(
                user.BusinessId.ToString(),
                businessId);
        }

        [Fact]
        public void Generate_ShouldContainUserRole()
        {
            // Arrange
            var service = CreateTokenService();

            var user = CreateUser();

            // Act
            var result = service.Generate(user);

            var token = ReadToken(result.Token);

            // Assert
            var roleClaim = token.Claims.FirstOrDefault(x =>
          x.Type == ClaimTypes.Role ||
          x.Type.Equals("role", StringComparison.OrdinalIgnoreCase));

            Assert.Equal(
                UserRole.Owner.ToString(),
                roleClaim.Value);
        }

        private static User CreateUser()
        {
            return new User(
                Guid.NewGuid(),
                "Juan Pérez",
                "juan@email.com",
                "hashed-password",
                UserRole.Owner);
        }

        [Fact]
        public void Generate_ShouldCreateValidSignedToken()
        {
            // Arrange
            const string key =
                "EstaEsUnaClaveDePruebaMuyLargaYSeguraParaJwt1234567890!";

            var settings = new JwtSettings
            {
                Issuer = "Facturacion.Api",
                Audience = "Facturacion.Desktop",
                Key = key,
                ExpirationMinutes = 30
            };

            var service = new JwtTokenService(
                Options.Create(settings));

            var user = CreateUser();

            // Act
            var result = service.Generate(user);

            var handler = new JwtSecurityTokenHandler();

            var validationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,

                    ValidateAudience = true,
                    ValidAudience = settings.Audience,

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(key)),

                    ClockSkew = TimeSpan.Zero
                };

            // Act
            var principal = handler.ValidateToken(
                result.Token,
                validationParameters,
                out var validatedToken);

            // Assert
            Assert.NotNull(principal);
            Assert.NotNull(validatedToken);
        }

        private static JwtSecurityToken ReadToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            return handler.ReadJwtToken(token);
        }



    }
}
