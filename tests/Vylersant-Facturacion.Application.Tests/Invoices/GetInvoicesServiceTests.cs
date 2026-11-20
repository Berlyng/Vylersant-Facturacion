using Vylersant_Facturacion.Application.Customers;
using Vylersant_Facturacion.Application.Invoices;
using Vylersant_Facturacion.Domain.Entities.Customers;
using Vylersant_Facturacion.Domain.Entities.Invoices;
using Vylersant_Facturacion.Domain.Entities.Products;

namespace Vylersant_Facturacion.Application.Tests.Invoices;

public sealed class GetInvoicesServiceTests
{
    [Fact]
    public async Task GetInvoices_ShouldReturnPagedInvoicesForBusiness()
    {
        var businessId = Guid.NewGuid();
        var first = CreateInvoice(businessId, 1, DateTime.UtcNow.AddDays(-1));
        var second = CreateInvoice(businessId, 2, DateTime.UtcNow);
        var repository = new FakeInvoiceRepository([first, second]);
        var service = new GetInvoicesService(repository);

        var result = await service.ExecuteAsync(businessId, page: 1, pageSize: 1);

        Assert.Single(result.Items);
        Assert.Equal(second.Id, result.Items[0].Id);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(businessId, repository.LastBusinessId);
    }

    [Fact]
    public async Task GetInvoiceById_ShouldReturnHistoricalLineAndCustomerDetails()
    {
        var businessId = Guid.NewGuid();
        var customer = new Customer(businessId, "Cliente histórico actual");
        var invoice = CreateInvoice(businessId, 1, DateTime.UtcNow, customer.Id);
        var repository = new FakeInvoiceRepository([invoice]);
        var service = new GetInvoiceByIdService(repository, new FakeCustomerRepository(customer));

        var result = await service.ExecuteAsync(invoice.Id, businessId);

        Assert.Equal(invoice.Id, result.Id);
        Assert.Equal("Cliente histórico actual", result.CustomerName);
        Assert.Equal("Producto", Assert.Single(result.Lines).ProductName);
        Assert.Equal(10m, result.Lines[0].UnitPrice);
    }

    private static Invoice CreateInvoice(Guid businessId, long number, DateTime issuedAt, Guid? customerId = null) =>
        new(businessId, Guid.NewGuid(), customerId, number, issuedAt,
            [new InvoiceLine(Guid.NewGuid(), "Producto", 1m, 10m)]);

    [Fact]
    public async Task GetInvoiceById_ShouldHideInvoiceFromAnotherBusiness()
    {
        var invoice = CreateInvoice(Guid.NewGuid(), 1, DateTime.UtcNow);
        var service = new GetInvoiceByIdService(
            new FakeInvoiceRepository([invoice]),
            new FakeCustomerRepository());

        await Assert.ThrowsAsync<InvoiceNotFoundException>(() => service.ExecuteAsync(invoice.Id, Guid.NewGuid()));
    }

    private static Invoice CreateInvoice(Guid businessId, long number, DateTime issuedAt) =>
        new(businessId, Guid.NewGuid(), null, number, issuedAt,
            [new InvoiceLine(Guid.NewGuid(), "Producto", 1m, 10m)]);

    private sealed class FakeInvoiceRepository(Invoice[] invoices) : IInvoiceRepository
    {
        public Guid LastBusinessId { get; private set; }
        public Task<Invoice?> GetByIdAsync(Guid invoiceId, Guid businessId, CancellationToken token = default)
        {
            LastBusinessId = businessId;
            return Task.FromResult(invoices.FirstOrDefault(invoice => invoice.Id == invoiceId && invoice.BusinessId == businessId));
        }
        public Task<IReadOnlyList<Invoice>> GetAllAsync(Guid businessId, CancellationToken token = default) => Task.FromResult<IReadOnlyList<Invoice>>(invoices.Where(invoice => invoice.BusinessId == businessId).ToList());
        public Task<IReadOnlyList<Invoice>> GetPageAsync(Guid businessId, int skip, int take, CancellationToken token = default)
        {
            LastBusinessId = businessId;
            return Task.FromResult<IReadOnlyList<Invoice>>(invoices.Where(invoice => invoice.BusinessId == businessId)
                .OrderByDescending(invoice => invoice.IssuedAtUtc).ThenByDescending(invoice => invoice.Number)
                .Skip(skip).Take(take).ToList());
        }
        public Task<long> GetCountAsync(Guid businessId, CancellationToken token = default) => Task.FromResult((long)invoices.Count(invoice => invoice.BusinessId == businessId));
        public Task<long> GetNextNumberAsync(Guid businessId, CancellationToken token = default) => Task.FromResult(1L);
        public Task AddAsync(Invoice invoice, CancellationToken token = default) => Task.CompletedTask;
    }

    private sealed class FakeCustomerRepository(params Customer[] customers) : ICustomerRepository
    {
        public Task<Customer?> GetByIdAsync(Guid customerId, Guid businessId, CancellationToken token = default) =>
            Task.FromResult(customers.FirstOrDefault(customer => customer.Id == customerId && customer.BusinessId == businessId));
        public Task<IReadOnlyList<Customer>> GetAllAsync(Guid businessId, CancellationToken token = default) =>
            Task.FromResult<IReadOnlyList<Customer>>(customers.Where(customer => customer.BusinessId == businessId).ToList());
        public Task AddAsync(Customer customer, CancellationToken token = default) => Task.CompletedTask;
    }
}

