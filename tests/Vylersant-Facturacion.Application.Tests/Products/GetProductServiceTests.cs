using Vylersant_Facturacion.Application.Products;
using Vylersant_Facturacion.Domain.Entities.Products;

namespace Vylersant_Facturacion.Application.Tests.Products;

public class GetProductsServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnProductsFromBusiness()
    {
        // Arrange
        var businessId = Guid.NewGuid();

        var products = new List<Product>
        {
            new(
                businessId,
                "Arroz",
                100m,
                "ARR-001",
                "Arroz premium"),

            new(
                businessId,
                "Aceite",
                250m,
                "ACE-001",
                null)
        };

        var repository =
            new FakeProductRepository(products);

        var service =
            new GetProductService(repository);

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
            product =>
                product.Name == "Arroz" &&
                product.Price == 100m);

        Assert.Contains(
            result,
            product =>
                product.Name == "Aceite" &&
                product.Price == 250m);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyList_WhenBusinessHasNoProducts()
    {
        // Arrange
        var repository =
            new FakeProductRepository([]);

        var service =
            new GetProductService(repository);

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
            new FakeProductRepository([]);

        var service =
            new GetProductService(repository);

        // Act
        var action = () =>
            service.ExecuteAsync(
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

            return Task.FromResult(product);
        }

        public Task<IReadOnlyList<Product>> GetAllAsync(
            Guid businessId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Product> result =
                _products
                    .Where(product =>
                        product.BusinessId == businessId)
                    .ToList();

            return Task.FromResult(result);
        }

        public Task AddAsync(
            Product product,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}