using Vylersant_Facturacion.Application.Customers;
using Vylersant_Facturacion.Application.Invoices;
using Vylersant_Facturacion.Application.Printing;
using Vylersant_Facturacion.Domain.Entities.Customers;
using Vylersant_Facturacion.Domain.Entities.Invoices;

namespace Vylersant_Facturacion.Application.Tests.Printing;

public sealed class PrintInvoiceReceiptServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldPrintExistingInvoiceHistoricalSnapshot()
    {
        var businessId = Guid.NewGuid();
        var customer = new Customer(businessId, "Cliente de prueba");
        var issuedAt = new DateTime(2026, 11, 27, 15, 30, 0, DateTimeKind.Utc);
        var invoice = new Invoice(businessId, Guid.NewGuid(), customer.Id, 42, issuedAt,
            [new InvoiceLine(Guid.NewGuid(), "Producto conservado", 2m, 12.50m)]);
        var printer = new SimulatedReceiptPrinter();
        var service = new PrintInvoiceReceiptService(
            new FakeInvoiceRepository(invoice), new FakeCustomerRepository(customer), printer);

        var result = await service.ExecuteAsync(invoice.Id, businessId);

        Assert.True(result.Succeeded);
        Assert.NotNull(printer.LastDocument);
        Assert.Equal(42, printer.LastDocument.InvoiceNumber);
        Assert.Equal(issuedAt, printer.LastDocument.IssuedAtUtc);
        Assert.Equal("Cliente de prueba", printer.LastDocument.CustomerName);
        Assert.Equal(25m, printer.LastDocument.Total);
        var line = Assert.Single(printer.LastDocument.Lines);
        Assert.Equal("Producto conservado", line.ProductName);
        Assert.Equal(12.50m, line.UnitPrice);
        Assert.Equal(25m, line.LineTotal);
        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPrinterFailureWithoutLosingTheError()
    {
        var invoice = MakeInvoice(Guid.NewGuid());
        var printer = new SimulatedReceiptPrinter { Failure = "Impresora sin papel." };
        var service = new PrintInvoiceReceiptService(new FakeInvoiceRepository(invoice), new FakeCustomerRepository(null), printer);

        var result = await service.ExecuteAsync(invoice.Id, invoice.BusinessId);

        Assert.False(result.Succeeded);
        Assert.Equal("Impresora sin papel.", result.ErrorMessage);
        Assert.NotNull(printer.LastDocument);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotPrintInvoiceFromAnotherBusiness()
    {
        var invoice = MakeInvoice(Guid.NewGuid());
        var printer = new SimulatedReceiptPrinter();
        var service = new PrintInvoiceReceiptService(new FakeInvoiceRepository(invoice), new FakeCustomerRepository(null), printer);

        await Assert.ThrowsAsync<InvoiceNotFoundException>(() => service.ExecuteAsync(invoice.Id, Guid.NewGuid()));

        Assert.Null(printer.LastDocument);
    }

    private static Invoice MakeInvoice(Guid businessId) => new(businessId, Guid.NewGuid(), null, 1, DateTime.UtcNow,
        [new InvoiceLine(Guid.NewGuid(), "Producto", 1m, 5m)]);

    private sealed class FakeInvoiceRepository(Invoice invoice) : IInvoiceRepository
    {
        public Task<Invoice?> GetByIdAsync(Guid invoiceId, Guid businessId, CancellationToken cancellationToken = default) =>
            Task.FromResult<Invoice?>(invoice.Id == invoiceId && invoice.BusinessId == businessId ? invoice : null);
        public Task<IReadOnlyList<Invoice>> GetAllAsync(Guid businessId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Invoice>>([]);
        public Task<IReadOnlyList<Invoice>> GetPageAsync(Guid businessId, int skip, int take, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Invoice>>([]);
        public Task<long> GetCountAsync(Guid businessId, CancellationToken cancellationToken = default) => Task.FromResult(0L);
        public Task<long> GetNextNumberAsync(Guid businessId, CancellationToken cancellationToken = default) => Task.FromResult(1L);
        public Task AddAsync(Invoice value, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeCustomerRepository(Customer? customer) : ICustomerRepository
    {
        public Task<Customer?> GetByIdAsync(Guid customerId, Guid businessId, CancellationToken cancellationToken = default) =>
            Task.FromResult<Customer?>(customer?.Id == customerId && customer.BusinessId == businessId ? customer : null);
        public Task<IReadOnlyList<Customer>> GetAllAsync(Guid businessId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Customer>>([]);
        public Task AddAsync(Customer value, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class SimulatedReceiptPrinter : IReceiptPrinter
    {
        public string? Failure { get; init; }
        public ReceiptPrintDocument? LastDocument { get; private set; }

        public Task<ReceiptPrintResult> PrintAsync(ReceiptPrintDocument document, CancellationToken cancellationToken = default)
        {
            LastDocument = document;
            var result = Failure is null
                ? ReceiptPrintResult.Success(DateTime.UtcNow, "SIM-001")
                : ReceiptPrintResult.Failure(DateTime.UtcNow, Failure, "SIM-001");
            return Task.FromResult(result);
        }
    }
}

