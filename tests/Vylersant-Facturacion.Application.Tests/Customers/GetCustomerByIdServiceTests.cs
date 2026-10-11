using Vylersant_Facturacion.Application.Customers;
using Vylersant_Facturacion.Domain.Entities.Customers;

namespace Vylersant_Facturacion.Application.Tests.Customers;

public class GetCustomerByIdServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnCustomer_WhenCustomerExistsInBusiness()
    {
        // Arrange
        var businessId = Guid.NewGuid();

        var customer = new Customer(
            businessId,
            "Juan Pérez",
            "Santo Domingo",
            "809-555-1234",
            "juan@email.com");

        var repository =
            new FakeCustomerRepository([customer]);

        var service =
            new GetCustomerByIdService(repository);

        // Act
        var result =
            await service.ExecuteAsync(
                customer.Id,
                businessId,
                CancellationToken.None);

        // Assert
        Assert.Equal(customer.Id, result.Id);
        Assert.Equal("Juan Pérez", result.Name);
        Assert.Equal("Santo Domingo", result.Address);
        Assert.Equal("809-555-1234", result.Phone);
        Assert.Equal("juan@email.com", result.Email);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenCustomerDoesNotExist()
    {
        var repository =
            new FakeCustomerRepository([]);

        var service =
            new GetCustomerByIdService(repository);

        var action = () =>
            service.ExecuteAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                CancellationToken.None);

        await Assert.ThrowsAsync<CustomerNotFoundException>(
            action);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenCustomerBelongsToAnotherBusiness()
    {
        var businessA = Guid.NewGuid();
        var businessB = Guid.NewGuid();

        var customer = new Customer(
            businessA,
            "Cliente A");

        var repository =
            new FakeCustomerRepository([customer]);

        var service =
            new GetCustomerByIdService(repository);

        var action = () =>
            service.ExecuteAsync(
                customer.Id,
                businessB,
                CancellationToken.None);

        await Assert.ThrowsAsync<CustomerNotFoundException>(
            action);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenCustomerIdIsEmpty()
    {
        var repository =
            new FakeCustomerRepository([]);

        var service =
            new GetCustomerByIdService(repository);

        var action = () =>
            service.ExecuteAsync(
                Guid.Empty,
                Guid.NewGuid(),
                CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentException>(
            action);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenBusinessIdIsEmpty()
    {
        var repository =
            new FakeCustomerRepository([]);

        var service =
            new GetCustomerByIdService(repository);

        var action = () =>
            service.ExecuteAsync(
                Guid.NewGuid(),
                Guid.Empty,
                CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentException>(
            action);
    }

    private sealed class FakeCustomerRepository
        : ICustomerRepository
    {
        private readonly IReadOnlyList<Customer> _customers;

        public FakeCustomerRepository(
            IReadOnlyList<Customer> customers)
        {
            _customers = customers;
        }

        public Task<Customer?> GetByIdAsync(
            Guid customerId,
            Guid businessId,
            CancellationToken cancellationToken = default)
        {
            var customer =
                _customers.FirstOrDefault(
                    customer =>
                        customer.Id == customerId &&
                        customer.BusinessId == businessId);

            return Task.FromResult(customer);
        }

        public Task<IReadOnlyList<Customer>> GetAllAsync(
            Guid businessId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Customer> result =
                _customers
                    .Where(customer =>
                        customer.BusinessId == businessId)
                    .ToList();

            return Task.FromResult(result);
        }

        public Task AddAsync(
            Customer customer,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}