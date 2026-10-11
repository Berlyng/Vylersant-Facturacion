using Vylersant_Facturacion.Application.Customers;
using Vylersant_Facturacion.Domain.Entities.Customers;

namespace Vylersant_Facturacion.Application.Tests.Customers;

public class GetCustomersServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnCustomersFromBusiness()
    {
        // Arrange
        var businessId = Guid.NewGuid();

        var customers = new List<Customer>
        {
            new(
                businessId,
                "Juan Pérez",
                "Santo Domingo",
                "809-555-1111",
                "juan@email.com"),

            new(
                businessId,
                "María Rodríguez",
                "Santiago",
                "809-555-2222",
                "maria@email.com")
        };

        var repository =
            new FakeCustomerRepository(customers);

        var service =
            new GetCustomersService(repository);

        // Act
        var result =
            await service.ExecuteAsync(
                businessId,
                CancellationToken.None);

        // Assert
        Assert.Equal(
            2,
            result.Count);

        Assert.Contains(
            result,
            customer =>
                customer.Name == "Juan Pérez");

        Assert.Contains(
            result,
            customer =>
                customer.Name == "María Rodríguez");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnOnlyCustomersFromRequestedBusiness()
    {
        // Arrange
        var businessA = Guid.NewGuid();
        var businessB = Guid.NewGuid();

        var customerA = new Customer(
            businessA,
            "Cliente A");

        var customerB = new Customer(
            businessB,
            "Cliente B");

        var repository =
            new FakeCustomerRepository(
                [customerA, customerB]);

        var service =
            new GetCustomersService(repository);

        // Act
        var result =
            await service.ExecuteAsync(
                businessA,
                CancellationToken.None);

        // Assert
        Assert.Single(result);

        Assert.Equal(
            customerA.Id,
            result[0].Id);

        Assert.DoesNotContain(
            result,
            customer =>
                customer.Id == customerB.Id);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyList_WhenBusinessHasNoCustomers()
    {
        // Arrange
        var repository =
            new FakeCustomerRepository([]);

        var service =
            new GetCustomersService(repository);

        // Act
        var result =
            await service.ExecuteAsync(
                Guid.NewGuid(),
                CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenBusinessIdIsEmpty()
    {
        // Arrange
        var repository =
            new FakeCustomerRepository([]);

        var service =
            new GetCustomersService(repository);

        // Act
        var action = () =>
            service.ExecuteAsync(
                Guid.Empty,
                CancellationToken.None);

        // Assert
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