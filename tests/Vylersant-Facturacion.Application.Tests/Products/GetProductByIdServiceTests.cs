using Vylersant_Facturacion.Application.Products;
using Vylersant_Facturacion.Domain.Entities.Products;

namespace Vylersant_Facturacion.Application.Tests.Products;

public class GetProductByIdServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnProduct_WhenProductExistsInBusiness()
    {
        // Arrange
        var businessId = Guid.NewGuid();

        var product = new Product(
            businessId,
            "Arroz",
            100m,
            "ARR-001",
            "Arroz premium");

        var repository =
            new FakeProductRepository(
                [product]);

        var service =
            new GetProductByIdService(
                repository);

        // Act
        var result =
            await service.ExecuteAsync(
                product.Id,
                businessId,
                CancellationToken.None);

        // Assert
        Assert.Equal(
            product.Id,
            result.Id);

        Assert.Equal(
            "Arroz",
            result.Name);

        Assert.Equal(
            100m,
            result.Price);

        Assert.Equal(
            "ARR-001",
            result.Sku);

        Assert.Equal(
            "Arroz premium",
            result.Description);

        Assert.True(
            result.IsActive);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var repository =
            new FakeProductRepository([]);

        var service =
            new GetProductByIdService(
                repository);

        // Act
        var action = () =>
            service.ExecuteAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ProductNotFoundException>(
            action);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenProductBelongsToAnotherBusiness()
    {
        // Arrange
        var businessA = Guid.NewGuid();
        var businessB = Guid.NewGuid();

        var product = new Product(
            businessA,
            "Producto negocio A",
            100m);

        var repository =
            new FakeProductRepository(
                [product]);

        var service =
            new GetProductByIdService(
                repository);

        // Act
        var action = () =>
            service.ExecuteAsync(
                product.Id,
                businessB,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ProductNotFoundException>(
            action);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenProductIdIsEmpty()
    {
        // Arrange
        var repository =
            new FakeProductRepository([]);

        var service =
            new GetProductByIdService(
                repository);

        // Act
        var action = () =>
            service.ExecuteAsync(
                Guid.Empty,
                Guid.NewGuid(),
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(
            action);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenBusinessIdIsEmpty()
    {
        // Arrange
        var repository =
            new FakeProductRepository([]);

        var service =
            new GetProductByIdService(
                repository);

        // Act
        var action = () =>
            service.ExecuteAsync(
                Guid.NewGuid(),
                Guid.Empty,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(
            action);
    }

    private sealed class FakeProductRepository
        : IProductRepository
    {
        private readonly IReadOnlyList<Product> _products;

        public FakeProductRepository(
            IReadOnlyList<Product> products)
        {
            _products = products;
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
                    .ToList();

            return Task.FromResult(
                products);
        }

        public Task AddAsync(
            Product product,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}