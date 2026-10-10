using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Bussinesses;
using Vylersant_Facturacion.Application.Registration;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Application.Users;
using Vylersant_Facturacion.Domain.Entities.Businesses;
using Vylersant_Facturacion.Domain.Entities.Users;

namespace Vylersant_Facturacion.Application.Tests.Registration
{
    public class RegisterBusinessServiceTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldCreateBusinessAndOwner_WhenRequestIsValid()
        {
            // Arrange
            var bussinesRepository = new FakeBusinessRepository();
            var userRepository = new FakeUserRepository();
            var passwordHasher = new FakePasswordHasher();
            var unitOfWork = new FakeUnitOfWork();

            var service = new RegisterBussinesService(
           bussinesRepository,
           unitOfWork,
           userRepository,
           passwordHasher
           );


            var request = new RegisterBusinessRequest(
                BussinesName: "Test Business",
                OwnerName: "John Doe",
                Email: "john.doe@example.com",
                Password: "password123");

            // Act
            var businessId = await service.ExecuteAsync(request);

            // Assert
            Assert.NotNull(bussinesRepository.AddedBusiness);
            Assert.NotNull(userRepository.AddedUser);
            Assert.Equal(businessId, bussinesRepository.AddedBusiness.Id);
            Assert.Equal(businessId, userRepository.AddedUser.BusinessId);
            Assert.Equal("HASHED:password123", userRepository.AddedUser.PasswordHash);
            Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrow_WhenEmailAlreadyExists()
        {
            // Arrange
            var businessRepository = new FakeBusinessRepository();

            var userRepository = new FakeUserRepository
            {
                ExistingUser = new User(
                    Guid.NewGuid(),
                    "Usuario Existente",
                    "juan@email.com",
                    "existing-hash",
                    UserRole.Employee)
            };

            var passwordHasher = new FakePasswordHasher();
            var unitOfWork = new FakeUnitOfWork();

            var service = new RegisterBussinesService(
                businessRepository,
                unitOfWork,
                userRepository,
                passwordHasher
                );

            var request = new RegisterBusinessRequest(
                "Negocio XYZ",
                "Juan Pérez",
                "juan@email.com",
                "Password123!");

            // Act
            var action = () => service.ExecuteAsync(request);

            // Assert
            await Assert.ThrowsAsync<EmailAlreadyExistsException>(action);

            Assert.Null(businessRepository.AddedBusiness);
            Assert.Null(userRepository.AddedUser);
            Assert.Equal(0, unitOfWork.SaveChangesCallCount);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldAssociateOwnerToCreatedBusinessId()
        {
            // Arrange
            var businessRepository = new FakeBusinessRepository();
            var userRepository = new FakeUserRepository();
            var passwordHasher = new FakePasswordHasher();
            var unitOfWork = new FakeUnitOfWork();

            var service = new RegisterBussinesService(
                businessRepository,
                unitOfWork,
                userRepository,
                passwordHasher
            );

            var request = new RegisterBusinessRequest(
                BussinesName: "Test Business",
                OwnerName: "John Doe",
                Email: "john.doe@example.com",
                Password: "password123"
            );

            // Act
            var businessId = await service.ExecuteAsync(request);

            // Assert
            Assert.NotNull(businessRepository.AddedBusiness);
            Assert.NotNull(userRepository.AddedUser);
            Assert.Equal(businessRepository.AddedBusiness.Id, userRepository.AddedUser.BusinessId);
            Assert.Equal(businessId, userRepository.AddedUser.BusinessId);
        }


        private sealed class FakeBusinessRepository : IBussinesRepository
        {
            public Business? AddedBusiness { get; private set; }

            public Task AddAsync(
                Business business,
                CancellationToken cancellationToken = default)
            {
                AddedBusiness = business;

                return Task.CompletedTask;
            }
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
                return Task.FromResult(ExistingUser!);
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
}
