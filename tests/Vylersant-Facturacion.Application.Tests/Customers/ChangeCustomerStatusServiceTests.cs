using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Customers;
using Vylersant_Facturacion.Domain.Entities.Customers;

namespace Vylersant_Facturacion.Application.Tests.Customers;

public class ChangeCustomerStatusServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldDeactivateCustomer_WhenIsActiveIsFalse()
    {
        // Arrange
        var businessId = Guid.NewGuid();

        var customer = new Customer(
            businessId,
            "Juan Pérez");

        var repository =
            new FakeCustomerRepository(customer);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new ChangeCustomerStatusService(
                repository,
                unitOfWork);

        // Act
        await service.ExecuteAsync(
            customer.Id,
            businessId,
            false,
            CancellationToken.None);

        // Assert
        Assert.False(customer.IsActive);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldActivateCustomer_WhenIsActiveIsTrue()
    {
        // Arrange
        var businessId = Guid.NewGuid();

        var customer = new Customer(
            businessId,
            "Juan Pérez");

        customer.Deactivate();

        var repository =
            new FakeCustomerRepository(customer);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new ChangeCustomerStatusService(
                repository,
                unitOfWork);

        // Act
        await service.ExecuteAsync(
            customer.Id,
            businessId,
            true,
            CancellationToken.None);

        // Assert
        Assert.True(customer.IsActive);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var repository =
            new FakeCustomerRepository(null);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new ChangeCustomerStatusService(
                repository,
                unitOfWork);

        // Act
        var action = () =>
            service.ExecuteAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                false,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<CustomerNotFoundException>(
            action);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenCustomerBelongsToAnotherBusiness()
    {
        // Arrange
        var businessA = Guid.NewGuid();
        var businessB = Guid.NewGuid();

        var customer = new Customer(
            businessA,
            "Cliente A");

        var repository =
            new FakeCustomerRepository(customer);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new ChangeCustomerStatusService(
                repository,
                unitOfWork);

        // Act
        var action = () =>
            service.ExecuteAsync(
                customer.Id,
                businessB,
                false,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<CustomerNotFoundException>(
            action);

        Assert.True(customer.IsActive);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    private sealed class FakeCustomerRepository
        : ICustomerRepository
    {
        private readonly Customer? _customer;

        public FakeCustomerRepository(
            Customer? customer)
        {
            _customer = customer;
        }

        public Task<Customer?> GetByIdAsync(
            Guid customerId,
            Guid businessId,
            CancellationToken cancellationToken = default)
        {
            if (_customer is null ||
                _customer.Id != customerId ||
                _customer.BusinessId != businessId)
            {
                return Task.FromResult<Customer?>(
                    null);
            }

            return Task.FromResult<Customer?>(
                _customer);
        }

        public Task<IReadOnlyList<Customer>> GetAllAsync(
            Guid businessId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Customer> customers = [];

            return Task.FromResult(customers);
        }

        public Task AddAsync(
            Customer customer,
            CancellationToken cancellationToken = default)
        {
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