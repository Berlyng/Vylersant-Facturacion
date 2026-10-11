using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Products;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Contracts.Products;
using Vylersant_Facturacion.Domain.Entities.Products;
using Vylersant_Facturacion.Domain.Entities.Users;


namespace Vylersant_Facturacion.IntegrationTests.Products;

public class ProductEndpointTests
    : IClassFixture<Vylersant_FacturacionApiFactory>
{
    private readonly Vylersant_FacturacionApiFactory _factory;

    public ProductEndpointTests(
        Vylersant_FacturacionApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_ShouldReturnOnlyProductsFromAuthenticatedBusiness()
    {
        // Arrange
        var businessA = Guid.NewGuid();
        var businessB = Guid.NewGuid();

        var productA1 = new Product(
            businessA,
            "Producto A1",
            100m);

        var productA2 = new Product(
            businessA,
            "Producto A2",
            200m);

        var productB = new Product(
            businessB,
            "Producto B",
            300m);

        var repository =
            new InMemoryProductRepository(
                [productA1, productA2, productB]);

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

        // Act
        var response =
            await client.GetAsync(
                "/api/products");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var products =
            await response.Content
                .ReadFromJsonAsync<List<ProductResponse>>();

        Assert.NotNull(products);

        Assert.Equal(
            2,
            products.Count);

        Assert.Contains(
            products,
            product =>
                product.Id == productA1.Id);

        Assert.Contains(
            products,
            product =>
                product.Id == productA2.Id);

        Assert.DoesNotContain(
            products,
            product =>
                product.Id == productB.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenProductBelongsToAnotherBusiness()
    {
        // Arrange
        var businessA = Guid.NewGuid();
        var businessB = Guid.NewGuid();

        var productB = new Product(
            businessB,
            "Producto privado B",
            500m);

        var repository =
            new InMemoryProductRepository(
                [productB]);

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

        // Act
        var response =
            await client.GetAsync(
                $"/api/products/{productB.Id}");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenProductBelongsToAnotherBusiness()
    {
        // Arrange
        var businessA = Guid.NewGuid();
        var businessB = Guid.NewGuid();

        var productB = new Product(
            businessB,
            "Producto B",
            500m);

        var repository =
            new InMemoryProductRepository(
                [productB]);

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
            new Application.Products.UpdateProductRequest(
                "Producto hackeado",
                1m,
                null,
                null);

        // Act
        var response =
            await client.PutAsJsonAsync(
                $"/api/products/{productB.Id}",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        Assert.Equal(
            "Producto B",
            productB.Name);

        Assert.Equal(
            500m,
            productB.Price);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ChangeStatus_ShouldReturnNotFound_WhenProductBelongsToAnotherBusiness()
    {
        // Arrange
        var businessA = Guid.NewGuid();
        var businessB = Guid.NewGuid();

        var productB = new Product(
            businessB,
            "Producto B",
            500m);

        var repository =
            new InMemoryProductRepository(
                [productB]);

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
            new ChangeProductStatusRequest(
                false);

        // Act
        var response =
            await client.PatchAsJsonAsync(
                $"/api/products/{productB.Id}/status",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        Assert.True(
            productB.IsActive);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Create_ShouldUseBusinessIdFromAuthenticatedUser()
    {
        // Arrange
        var businessA =
            Guid.NewGuid();

        var repository =
            new InMemoryProductRepository([]);

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
            new Application.Products.CreateProductRequest(
                "Producto nuevo",
                125m,
                "PROD-001",
                "Producto del negocio A");

        // Act
        var response =
            await client.PostAsJsonAsync(
                "/api/products",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        Assert.NotNull(
            repository.AddedProduct);

        Assert.Equal(
            businessA,
            repository.AddedProduct.BusinessId);

        Assert.Equal(
            "Producto nuevo",
            repository.AddedProduct.Name);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    private WebApplicationFactory<Program> CreateFactory(
        InMemoryProductRepository productRepository,
        FakeUnitOfWork unitOfWork)
    {
        return _factory.WithWebHostBuilder(
            builder =>
            {
                builder.ConfigureTestServices(
                    services =>
                    {
                        services.RemoveAll<IProductRepository>();
                        services.RemoveAll<IUnitOfWork>();

                        services.AddSingleton<IProductRepository>(
                            productRepository);

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

    private sealed class InMemoryProductRepository
        : IProductRepository
    {
        private readonly List<Product> _products;

        public Product? AddedProduct { get; private set; }

        public InMemoryProductRepository(
            IEnumerable<Product> products)
        {
            _products = products.ToList();
        }

        public Task<Product?> GetByIdAsync(
            Guid productId,
            Guid businessId,
            CancellationToken cancellationToken = default)
        {
            var product =
                _products.FirstOrDefault(
                    product =>
                        product.Id == productId &&
                        product.BusinessId == businessId);

            return Task.FromResult(
                product);
        }

        public Task<IReadOnlyList<Product>> GetAllAsync(
            Guid businessId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Product> products =
                _products
                    .Where(product =>
                        product.BusinessId == businessId)
                    .OrderBy(product =>
                        product.Name)
                    .ToList();

            return Task.FromResult(
                products);
        }

        public Task AddAsync(
            Product product,
            CancellationToken cancellationToken = default)
        {
            AddedProduct = product;

            _products.Add(product);

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