using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Products;
using Vylersant_Facturacion.Domain.Entities.Products;

namespace Vylersant_Facturacion.Application.Tests.Products;

public class ChangeProductStatusServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldDeactivateProduct_WhenIsActiveIsFalse()
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
            new ChangeProductStatusService(
                repository,
                unitOfWork);

        // Act
        await service.ExecuteAsync(
            product.Id,
            businessId,
            false,
            CancellationToken.None);

        // Assert
        Assert.False(product.IsActive);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldActivateProduct_WhenIsActiveIsTrue()
    {
        // Arrange
        var businessId = Guid.NewGuid();

        var product = new Product(
            businessId,
            "Producto",
            100m);

        product.Deactivate();

        var repository =
            new FakeProductRepository(product);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new ChangeProductStatusService(
                repository,
                unitOfWork);

        // Act
        await service.ExecuteAsync(
            product.Id,
            businessId,
            true,
            CancellationToken.None);

        // Assert
        Assert.True(product.IsActive);

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
            new ChangeProductStatusService(
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
            new ChangeProductStatusService(
                repository,
                unitOfWork);

        // Act
        var action = () =>
            service.ExecuteAsync(
                product.Id,
                businessB,
                false,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ProductNotFoundException>(
            action);

        Assert.True(product.IsActive);

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
            if (_product is null ||
                _product.Id != productId ||
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