using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Customers;
using Vylersant_Facturacion.Application.Invoices;
using Vylersant_Facturacion.Application.Products;
using Vylersant_Facturacion.Domain.Entities.Customers;
using Vylersant_Facturacion.Domain.Entities.Invoices;
using Vylersant_Facturacion.Domain.Entities.Products;

namespace Vylersant_Facturacion.Application.Tests.Invoices;

public sealed class CreateInvoiceServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldCreateInvoiceUsingCurrentProductPrices()
    {
        var businessId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var customer = new Customer(businessId, "Cliente");
        var product = new Product(businessId, "Producto", 12.50m);
        var invoices = new FakeInvoiceRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new CreateInvoiceService(invoices, new FakeProductRepository(product),
            new FakeCustomerRepository(customer), unitOfWork, TimeProvider.System);

        var result = await service.ExecuteAsync(businessId, userId,
            new CreateInvoiceRequest(customer.Id, [new CreateInvoiceLineRequest(product.Id, 2m)]));

        Assert.Equal(1, result.Number);
        Assert.Equal(25m, result.Total);
        Assert.Equal(result.InvoiceId, invoices.Invoice!.Id);
        Assert.Equal(userId, invoices.Invoice.IssuedByUserId);
        Assert.Equal("Producto", invoices.Invoice.Lines.Single().ProductName);
        Assert.Equal(12.50m, invoices.Invoice.Lines.Single().UnitPrice);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRejectProductFromAnotherBusiness()
    {
        var service = new CreateInvoiceService(new FakeInvoiceRepository(),
            new FakeProductRepository(new Product(Guid.NewGuid(), "Ajeno", 10m)),
            new FakeCustomerRepository(), new FakeUnitOfWork(), TimeProvider.System);

        await Assert.ThrowsAsync<ProductNotFoundException>(() => service.ExecuteAsync(
            Guid.NewGuid(), Guid.NewGuid(),
            new CreateInvoiceRequest(null, [new CreateInvoiceLineRequest(Guid.NewGuid(), 1m)])));
    }

    private sealed class FakeInvoiceRepository : IInvoiceRepository
    {
        public Invoice? Invoice { get; private set; }
        public Task<Invoice?> GetByIdAsync(Guid id, Guid businessId, CancellationToken token = default) => Task.FromResult<Invoice?>(null);
        public Task<IReadOnlyList<Invoice>> GetAllAsync(Guid businessId, CancellationToken token = default) => Task.FromResult<IReadOnlyList<Invoice>>([]);
        public Task<long> GetNextNumberAsync(Guid businessId, CancellationToken token = default) => Task.FromResult(1L);
        public Task AddAsync(Invoice invoice, CancellationToken token = default) { Invoice = invoice; return Task.CompletedTask; }
    }

    private sealed class FakeProductRepository(params Product[] products) : IProductRepository
    {
        public Task<Product?> GetByIdAsync(Guid id, Guid businessId, CancellationToken token = default) =>
            Task.FromResult(products.FirstOrDefault(product => product.Id == id && product.BusinessId == businessId));
        public Task<IReadOnlyList<Product>> GetAllAsync(Guid businessId, CancellationToken token = default) =>
            Task.FromResult<IReadOnlyList<Product>>(products.Where(product => product.BusinessId == businessId).ToList());
        public Task AddAsync(Product product, CancellationToken token = default) => Task.CompletedTask;
    }

    private sealed class FakeCustomerRepository(params Customer[] customers) : ICustomerRepository
    {
        public Task<Customer?> GetByIdAsync(Guid id, Guid businessId, CancellationToken token = default) =>
            Task.FromResult(customers.FirstOrDefault(customer => customer.Id == id && customer.BusinessId == businessId));
        public Task<IReadOnlyList<Customer>> GetAllAsync(Guid businessId, CancellationToken token = default) =>
            Task.FromResult<IReadOnlyList<Customer>>(customers.Where(customer => customer.BusinessId == businessId).ToList());
        public Task AddAsync(Customer customer, CancellationToken token = default) => Task.CompletedTask;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }
        public Task<int> SaveChangesAsync(CancellationToken token = default) { SaveCount++; return Task.FromResult(1); }
    }
}

