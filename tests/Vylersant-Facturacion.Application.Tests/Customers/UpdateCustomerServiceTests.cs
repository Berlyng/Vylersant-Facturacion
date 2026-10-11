using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Customers;
using Vylersant_Facturacion.Domain.Entities.Customers;

namespace Vylersant_Facturacion.Application.Tests.Customers;

public class UpdateCustomerServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldUpdateCustomer_WhenCustomerExists()
    {
        // Arrange
        var businessId = Guid.NewGuid();

        var customer = new Customer(
            businessId,
            "Juan Pérez",
            "Santo Domingo",
            "809-555-1111",
            "juan@email.com");

        var repository =
            new FakeCustomerRepository(customer);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new UpdateCustomerService(
                repository,
                unitOfWork);

        var request =
            new UpdateCustomerRequest(
                "Pedro Martínez",
                "Santiago",
                "809-555-9999",
                "PEDRO@EMAIL.COM");

        // Act
        await service.ExecuteAsync(
            customer.Id,
            businessId,
            request,
            CancellationToken.None);

        // Assert
        Assert.Equal(
            "Pedro Martínez",
            customer.Name);

        Assert.Equal(
            "Santiago",
            customer.Address);

        Assert.Equal(
            "809-555-9999",
            customer.Phone);

        Assert.Equal(
            "pedro@email.com",
            customer.Email);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAllowClearingOptionalFields()
    {
        // Arrange
        var businessId = Guid.NewGuid();

        var customer = new Customer(
            businessId,
            "Juan Pérez",
            "Santo Domingo",
            "809-555-1111",
            "juan@email.com");

        var repository =
            new FakeCustomerRepository(customer);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new UpdateCustomerService(
                repository,
                unitOfWork);

        var request =
            new UpdateCustomerRequest(
                "Juan Pérez",
                null,
                " ",
                "");

        // Act
        await service.ExecuteAsync(
            customer.Id,
            businessId,
            request,
            CancellationToken.None);

        // Assert
        Assert.Null(customer.Address);
        Assert.Null(customer.Phone);
        Assert.Null(customer.Email);

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
            new UpdateCustomerService(
                repository,
                unitOfWork);

        var request =
            new UpdateCustomerRequest(
                "Cliente",
                null,
                null,
                null);

        // Act
        var action = () =>
            service.ExecuteAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                request,
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
            new UpdateCustomerService(
                repository,
                unitOfWork);

        var request =
            new UpdateCustomerRequest(
                "Intento modificar",
                null,
                null,
                null);

        // Act
        var action = () =>
            service.ExecuteAsync(
                customer.Id,
                businessB,
                request,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<CustomerNotFoundException>(
            action);

        Assert.Equal(
            "Cliente A",
            customer.Name);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenNameIsEmpty()
    {
        // Arrange
        var businessId = Guid.NewGuid();

        var customer = new Customer(
            businessId,
            "Cliente original");

        var repository =
            new FakeCustomerRepository(customer);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new UpdateCustomerService(
                repository,
                unitOfWork);

        var request =
            new UpdateCustomerRequest(
                " ",
                null,
                null,
                null);

        // Act
        var action = () =>
            service.ExecuteAsync(
                customer.Id,
                businessId,
                request,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(
            action);

        Assert.Equal(
            "Cliente original",
            customer.Name);

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
                return Task.FromResult<Customer?>(null);
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