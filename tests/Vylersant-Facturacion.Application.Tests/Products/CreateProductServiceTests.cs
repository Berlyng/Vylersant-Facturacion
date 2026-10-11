using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Products;
using Vylersant_Facturacion.Domain.Entities.Products;

namespace Vylersant_Facturacion.Application.Tests.Products;

public class CreateProductServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldCreateProduct_WhenDataIsValid()
    {
        // Arrange
        var businessId = Guid.NewGuid();

        var productRepository =
            new FakeProductRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new CreateProductService(
                productRepository,
                unitOfWork);

        var request =
            new CreateProductRequest(
                "Arroz Premium",
                150m,
                "ARR-001",
                "Arroz premium");

        // Act
        var result =
            await service.ExecuteAsync(
                businessId,
                request,
                CancellationToken.None);

        // Assert
        Assert.NotEqual(
            Guid.Empty,
            result.ProductId);

        Assert.NotNull(
            productRepository.AddedProduct);

        Assert.Equal(
            result.ProductId,
            productRepository.AddedProduct.Id);

        Assert.Equal(
            businessId,
            productRepository.AddedProduct.BusinessId);

        Assert.Equal(
            "Arroz Premium",
            productRepository.AddedProduct.Name);

        Assert.Equal(
            150m,
            productRepository.AddedProduct.Price);

        Assert.Equal(
            "ARR-001",
            productRepository.AddedProduct.Sku);

        Assert.Equal(
            "Arroz premium",
            productRepository.AddedProduct.Description);

        Assert.True(
            productRepository.AddedProduct.IsActive);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenPriceIsNegative()
    {
        // Arrange
        var productRepository =
            new FakeProductRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new CreateProductService(
                productRepository,
                unitOfWork);

        var request =
            new CreateProductRequest(
                "Producto",
                -10m,
                null,
                null);

        // Act
        var action = () =>
            service.ExecuteAsync(
                Guid.NewGuid(),
                request,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<
            ArgumentOutOfRangeException>(
                action);

        Assert.Null(
            productRepository.AddedProduct);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenBusinessIdIsEmpty()
    {
        var productRepository =
            new FakeProductRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new CreateProductService(
                productRepository,
                unitOfWork);

        var request =
            new CreateProductRequest(
                "Producto",
                100m,
                null,
                null);

        var action = () =>
            service.ExecuteAsync(
                Guid.Empty,
                request,
                CancellationToken.None);

        await Assert.ThrowsAsync<
            ArgumentException>(
                action);

        Assert.Null(
            productRepository.AddedProduct);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    private sealed class FakeProductRepository
        : IProductRepository
    {
        public Product? AddedProduct { get; private set; }

        public Task<Product?> GetByIdAsync(
            Guid productId,
            Guid businessId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Product?>(
                null);
        }

        public Task<IReadOnlyList<Product>> GetAllAsync(
            Guid businessId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Product> products =
                [];

            return Task.FromResult(
                products);
        }

        public Task AddAsync(
            Product product,
            CancellationToken cancellationToken = default)
        {
            AddedProduct = product;

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