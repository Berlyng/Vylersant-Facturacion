using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Customers;
using Vylersant_Facturacion.Domain.Entities.Customers;

namespace Vylersant_Facturacion.Application.Tests.Customers;

public class CreateCustomerServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldCreateCustomer_WhenDataIsValid()
    {
        // Arrange
        var businessId =
            Guid.NewGuid();

        var repository =
            new FakeCustomerRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new CreateCustomerService(
                repository,
                unitOfWork);

        var request =
            new CreateCustomerRequest(
                "Juan Pérez",
                "Santo Domingo",
                "809-555-1234",
                "JUAN@EMAIL.COM");

        // Act
        var result =
            await service.ExecuteAsync(
                businessId,
                request,
                CancellationToken.None);

        // Assert
        Assert.NotEqual(
            Guid.Empty,
            result.CustomerId);

        Assert.NotNull(
            repository.AddedCustomer);

        Assert.Equal(
            result.CustomerId,
            repository.AddedCustomer.Id);

        Assert.Equal(
            businessId,
            repository.AddedCustomer.BusinessId);

        Assert.Equal(
            "Juan Pérez",
            repository.AddedCustomer.Name);

        Assert.Equal(
            "Santo Domingo",
            repository.AddedCustomer.Address);

        Assert.Equal(
            "809-555-1234",
            repository.AddedCustomer.Phone);

        Assert.Equal(
            "juan@email.com",
            repository.AddedCustomer.Email);

        Assert.True(
            repository.AddedCustomer.IsActive);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenNameIsEmpty()
    {
        // Arrange
        var repository =
            new FakeCustomerRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new CreateCustomerService(
                repository,
                unitOfWork);

        var request =
            new CreateCustomerRequest(
                " ",
                null,
                null,
                null);

        // Act
        var action = () =>
            service.ExecuteAsync(
                Guid.NewGuid(),
                request,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(
            action);

        Assert.Null(
            repository.AddedCustomer);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenBusinessIdIsEmpty()
    {
        // Arrange
        var repository =
            new FakeCustomerRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new CreateCustomerService(
                repository,
                unitOfWork);

        var request =
            new CreateCustomerRequest(
                "Juan Pérez",
                null,
                null,
                null);

        // Act
        var action = () =>
            service.ExecuteAsync(
                Guid.Empty,
                request,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(
            action);

        Assert.Null(
            repository.AddedCustomer);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    private sealed class FakeCustomerRepository
        : ICustomerRepository
    {
        public Customer? AddedCustomer { get; private set; }

        public Task<Customer?> GetByIdAsync(
            Guid customerId,
            Guid businessId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Customer?>(
                null);
        }

        public Task<IReadOnlyList<Customer>> GetAllAsync(
            Guid businessId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Customer> customers = [];

            return Task.FromResult(
                customers);
        }

        public Task AddAsync(
            Customer customer,
            CancellationToken cancellationToken = default)
        {
            AddedCustomer = customer;

            return Task.CompletedTask;
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
}