using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Customers;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Contracts.Customers;
using Vylersant_Facturacion.Domain.Entities.Customers;
using Vylersant_Facturacion.Domain.Entities.Users;

namespace Vylersant_Facturacion.IntegrationTests.Customers;

public class CustomerEndpointTests
    : IClassFixture<Vylersant_FacturacionApiFactory>
{
    private readonly Vylersant_FacturacionApiFactory _factory;

    public CustomerEndpointTests(
        Vylersant_FacturacionApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_ShouldReturnOnlyCustomersFromAuthenticatedBusiness()
    {
        var businessA = Guid.NewGuid();
        var businessB = Guid.NewGuid();

        var customerA1 = new Customer(
            businessA,
            "Cliente A1");

        var customerA2 = new Customer(
            businessA,
            "Cliente A2");

        var customerB = new Customer(
            businessB,
            "Cliente B");

        var repository =
            new InMemoryCustomerRepository(
                [customerA1, customerA2, customerB]);

        var unitOfWork =
            new FakeUnitOfWork();

        using var factory =
            CreateFactory(
                repository,
                unitOfWork);

        var userA =
            CreateUser(
                businessA,
                "user-a@test.com");

        using var client =
            CreateAuthenticatedClient(
                factory,
                userA);

        var response =
            await client.GetAsync(
                "/api/customers");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var customers =
            await response.Content
                .ReadFromJsonAsync<List<CustomerResponse>>();

        Assert.NotNull(customers);

        Assert.Equal(
            2,
            customers.Count);

        Assert.Contains(
            customers,
            customer =>
                customer.Id == customerA1.Id);

        Assert.Contains(
            customers,
            customer =>
                customer.Id == customerA2.Id);

        Assert.DoesNotContain(
            customers,
            customer =>
                customer.Id == customerB.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenCustomerBelongsToAnotherBusiness()
    {
        var businessA = Guid.NewGuid();
        var businessB = Guid.NewGuid();

        var customerB = new Customer(
            businessB,
            "Cliente privado B");

        var repository =
            new InMemoryCustomerRepository(
                [customerB]);

        var unitOfWork =
            new FakeUnitOfWork();

        using var factory =
            CreateFactory(
                repository,
                unitOfWork);

        var userA =
            CreateUser(
                businessA,
                "user-a@test.com");

        using var client =
            CreateAuthenticatedClient(
                factory,
                userA);

        var response =
            await client.GetAsync(
                $"/api/customers/{customerB.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenCustomerBelongsToAnotherBusiness()
    {
        var businessA = Guid.NewGuid();
        var businessB = Guid.NewGuid();

        var customerB = new Customer(
            businessB,
            "Cliente B");

        var repository =
            new InMemoryCustomerRepository(
                [customerB]);

        var unitOfWork =
            new FakeUnitOfWork();

        using var factory =
            CreateFactory(
                repository,
                unitOfWork);

        var userA =
            CreateUser(
                businessA,
                "user-a@test.com");

        using var client =
            CreateAuthenticatedClient(
                factory,
                userA);

        var request =
            new Application.Customers.UpdateCustomerRequest(
                "Cliente hackeado",
                null,
                null,
                null);

        var response =
            await client.PutAsJsonAsync(
                $"/api/customers/{customerB.Id}",
                request);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        Assert.Equal(
            "Cliente B",
            customerB.Name);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ChangeStatus_ShouldReturnNotFound_WhenCustomerBelongsToAnotherBusiness()
    {
        var businessA = Guid.NewGuid();
        var businessB = Guid.NewGuid();

        var customerB = new Customer(
            businessB,
            "Cliente B");

        var repository =
            new InMemoryCustomerRepository(
                [customerB]);

        var unitOfWork =
            new FakeUnitOfWork();

        using var factory =
            CreateFactory(
                repository,
                unitOfWork);

        var userA =
            CreateUser(
                businessA,
                "user-a@test.com");

        using var client =
            CreateAuthenticatedClient(
                factory,
                userA);

        var request =
            new ChangeCustomerStatusRequest(
                false);

        var response =
            await client.PatchAsJsonAsync(
                $"/api/customers/{customerB.Id}/status",
                request);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        Assert.True(
            customerB.IsActive);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Create_ShouldUseBusinessIdFromAuthenticatedUser()
    {
        var businessA =
            Guid.NewGuid();

        var repository =
            new InMemoryCustomerRepository([]);

        var unitOfWork =
            new FakeUnitOfWork();

        using var factory =
            CreateFactory(
                repository,
                unitOfWork);

        var userA =
            CreateUser(
                businessA,
                "user-a@test.com");

        using var client =
            CreateAuthenticatedClient(
                factory,
                userA);

        var request =
            new Application.Customers.CreateCustomerRequest(
                "Cliente nuevo",
                "Santo Domingo",
                "809-555-1234",
                "cliente@email.com");

        var response =
            await client.PostAsJsonAsync(
                "/api/customers",
                request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        Assert.NotNull(
            repository.AddedCustomer);

        Assert.Equal(
            businessA,
            repository.AddedCustomer.BusinessId);

        Assert.Equal(
            "Cliente nuevo",
            repository.AddedCustomer.Name);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task GetAll_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        // Arrange
        using var client =
            _factory.CreateClient();

        // Act
        var response =
            await client.GetAsync(
                "/api/customers");

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        // Arrange
        using var client =
            _factory.CreateClient();

        var request =
            new Application.Customers.CreateCustomerRequest(
                "Cliente",
                null,
                null,
                null);

        // Act
        var response =
            await client.PostAsJsonAsync(
                "/api/customers",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
    [Fact]
    public async Task GetById_ShouldReturnProblemDetails_WhenCustomerDoesNotExist()
    {
        // Arrange
        var businessId =
            Guid.NewGuid();

        var repository =
            new InMemoryCustomerRepository([]);

        var unitOfWork =
            new FakeUnitOfWork();

        using var factory =
            CreateFactory(
                repository,
                unitOfWork);

        var user =
            CreateUser(
                businessId,
                "user@test.com");

        using var client =
            CreateAuthenticatedClient(
                factory,
                user);

        // Act
        var response =
            await client.GetAsync(
                $"/api/customers/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);

        Assert.Equal(
            StatusCodes.Status404NotFound,
            problem.Status);

        Assert.Equal(
            "Cliente no encontrado",
            problem.Title);
    }

    private WebApplicationFactory<Program> CreateFactory(
        InMemoryCustomerRepository customerRepository,
        FakeUnitOfWork unitOfWork)
    {
        return _factory.WithWebHostBuilder(
            builder =>
            {
                builder.ConfigureTestServices(
                    services =>
                    {
                        services.RemoveAll<ICustomerRepository>();
                        services.RemoveAll<IUnitOfWork>();

                        services.AddSingleton<ICustomerRepository>(
                            customerRepository);

                        services.AddSingleton<IUnitOfWork>(
                            unitOfWork);
                    });
            });
    }

    private static User CreateUser(
        Guid businessId,
        string email)
    {
        return new User(
            businessId,
            "Integration User",
            email,
            "hashed-password",
            UserRole.Owner);
    }

    private static HttpClient CreateAuthenticatedClient(
        WebApplicationFactory<Program> factory,
        User user)
    {
        var client =
            factory.CreateClient();

        using var scope =
            factory.Services.CreateScope();

        var tokenService =
            scope.ServiceProvider
                .GetRequiredService<ITokenService>();

        var accessToken =
            tokenService.Generate(user);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken.Token);

        return client;
    }

    private sealed class InMemoryCustomerRepository
        : ICustomerRepository
    {
        private readonly List<Customer> _customers;

        public Customer? AddedCustomer { get; private set; }

        public InMemoryCustomerRepository(
            IEnumerable<Customer> customers)
        {
            _customers = customers.ToList();
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
            IReadOnlyList<Customer> customers =
                _customers
                    .Where(customer =>
                        customer.BusinessId == businessId)
                    .OrderBy(customer =>
                        customer.Name)
                    .ToList();

            return Task.FromResult(customers);
        }

        public Task AddAsync(
            Customer customer,
            CancellationToken cancellationToken = default)
        {
            AddedCustomer = customer;
            _customers.Add(customer);

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