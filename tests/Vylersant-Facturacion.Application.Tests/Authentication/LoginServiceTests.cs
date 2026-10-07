using System;
using System.Collections.Generic;
using System.Text;
using Vylersant_Facturacion.Application.Authentication;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Application.Users;
using Vylersant_Facturacion.Domain.Entities.Users;

namespace Vylersant_Facturacion.Application.Tests.Authentication
{
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
                UserRole.Owner
            );

            var userRepository = new FakeUserRepository { ExistingUser = user };
            var passwordHasher = new FakePasswordHasher();

            var service = new LoginService(userRepository, passwordHasher);

            var request = new LoginRequest("test@example.com", rawPassword);
            
            var result = await service.ExecuteAsync(request, CancellationToken.None);


            Assert.NotNull(result);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(user.BusinessId, result.BusinessId);
            Assert.Equal(user.Name, result.Name);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(UserRole.Owner, result.Role);


        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowException_WhenEmailDoesNotExist()
        {
            // Arrange
            var userRepository = new FakeUserRepository { ExistingUser = null };
            var passwordHasher = new FakePasswordHasher();
            var service = new LoginService(userRepository, passwordHasher);
            var request = new LoginRequest("nonexistent@example.com", "hashedpassword");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidCredentialsException>(() => service.ExecuteAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowException_WhenPasswordIsIncorrect()
        {
            // Arrange
            var user = new User(
                Guid.NewGuid(),
                "John Doe",
                "test@example.com",
                "correctpassword",
                UserRole.Owner
            );

            var userRepository = new FakeUserRepository { ExistingUser = user };
            var passwordHasher = new FakePasswordHasher();

            var service = new LoginService(userRepository, passwordHasher);

            var request = new LoginRequest("test@example.com", "incorrectpassword");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidCredentialsException>(() => service.ExecuteAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowException_WhenUserIsInactive()
        {
            // Arrange
            var user = new User(
                Guid.NewGuid(),
                "John Doe",
                "test@example.com",
                "hashedpassword",
                UserRole.Owner
            );
            user.Deactivate(); 

            var userRepository = new FakeUserRepository { ExistingUser = user };
            var passwordHasher = new FakePasswordHasher();

            var service = new LoginService(userRepository, passwordHasher);

            var request = new LoginRequest("test@example.com", "hashedpassword");

            // Act & Assert
            await Assert.ThrowsAsync<InactiveUserException>(() => service.ExecuteAsync(request, CancellationToken.None));
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

    }
}
