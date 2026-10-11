using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Products;
using Vylersant_Facturacion.Domain.Entities.Products;

namespace Vylersant_Facturacion.Application.Tests.Products;

public class UpdateProductServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldUpdateProduct_WhenProductExists()
    {
        // Arrange
        var businessId = Guid.NewGuid();

        var product = new Product(
            businessId,
            "Producto viejo",
            100m,
            "OLD-001",
            "Descripción vieja");

        var repository =
            new FakeProductRepository(product);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new UpdateProductService(
                repository,
                unitOfWork);

        var request =
            new UpdateProductRequest(
                "Producto nuevo",
                250m,
                "NEW-001",
                "Descripción nueva");

        // Act
        await service.ExecuteAsync(
            product.Id,
            businessId,
            request,
            CancellationToken.None);

        // Assert
        Assert.Equal(
            "Producto nuevo",
            product.Name);

        Assert.Equal(
            250m,
            product.Price);

        Assert.Equal(
            "NEW-001",
            product.Sku);

        Assert.Equal(
            "Descripción nueva",
            product.Description);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var repository =
            new FakeProductRepository(null);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new UpdateProductService(
                repository,
                unitOfWork);

        var request =
            new UpdateProductRequest(
                "Producto",
                100m,
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
        await Assert.ThrowsAsync<ProductNotFoundException>(
            action);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenProductBelongsToAnotherBusiness()
    {
        // Arrange
        var businessA = Guid.NewGuid();
        var businessB = Guid.NewGuid();

        var product = new Product(
            businessA,
            "Producto",
            100m);

        var repository =
            new FakeProductRepository(product);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new UpdateProductService(
                repository,
                unitOfWork);

        var request =
            new UpdateProductRequest(
                "Intento modificar",
                200m,
                null,
                null);

        // Act
        var action = () =>
            service.ExecuteAsync(
                product.Id,
                businessB,
                request,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ProductNotFoundException>(
            action);

        Assert.Equal(
            "Producto",
            product.Name);

        Assert.Equal(
            100m,
            product.Price);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenPriceIsNegative()
    {
        // Arrange
        var businessId = Guid.NewGuid();

        var product = new Product(
            businessId,
            "Producto",
            100m);

        var repository =
            new FakeProductRepository(product);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new UpdateProductService(
                repository,
                unitOfWork);

        var request =
            new UpdateProductRequest(
                "Producto",
                -10m,
                null,
                null);

        // Act
        var action = () =>
            service.ExecuteAsync(
                product.Id,
                businessId,
                request,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            action);

        Assert.Equal(
            100m,
            product.Price);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    private sealed class FakeProductRepository
        : IProductRepository
    {
        private readonly Product? _product;

        public FakeProductRepository(
            Product? product)
        {
            _product = product;
        }

        public Task<Product?> GetByIdAsync(
            Guid productId,
            Guid businessId,
            CancellationToken cancellationToken = default)
        {
            if (_product is null)
            {
                return Task.FromResult<Product?>(
                    null);
            }

            if (_product.Id != productId ||
                _product.BusinessId != businessId)
            {
                return Task.FromResult<Product?>(
                    null);
            }

            return Task.FromResult<Product?>(
                _product);
        }

        public Task<IReadOnlyList<Product>> GetAllAsync(
            Guid businessId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Product> products = [];

            return Task.FromResult(products);
        }

        public Task AddAsync(
            Product product,
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