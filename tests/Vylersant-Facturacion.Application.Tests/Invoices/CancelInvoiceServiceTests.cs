using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Invoices;
using Vylersant_Facturacion.Domain.Entities.Invoices;

namespace Vylersant_Facturacion.Application.Tests.Invoices;

public sealed class CancelInvoiceServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldCancelInvoiceAndPreserveNumber()
    {
        var businessId = Guid.NewGuid();
        var invoice = CreateInvoice(businessId);
        var repository = new FakeInvoiceRepository(invoice);
        var unitOfWork = new FakeUnitOfWork();
        var cancelledAt = new DateTimeOffset(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);
        var service = new CancelInvoiceService(repository, unitOfWork, new FixedTimeProvider(cancelledAt));

        await service.ExecuteAsync(invoice.Id, businessId);

        Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
        Assert.Equal(17, invoice.Number);
        Assert.Equal(cancelledAt.UtcDateTime, invoice.CancelledAtUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotSaveOrRevealInvoiceFromAnotherBusiness()
    {
        var invoice = CreateInvoice(Guid.NewGuid());
        var unitOfWork = new FakeUnitOfWork();
        var service = new CancelInvoiceService(
            new FakeInvoiceRepository(invoice), unitOfWork, TimeProvider.System);

        await Assert.ThrowsAsync<InvoiceNotFoundException>(() =>
            service.ExecuteAsync(invoice.Id, Guid.NewGuid()));

        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldBeIdempotent_WhenInvoiceIsAlreadyCancelled()
    {
        var invoice = CreateInvoice(Guid.NewGuid());
        var unitOfWork = new FakeUnitOfWork();
        var service = new CancelInvoiceService(
            new FakeInvoiceRepository(invoice), unitOfWork, TimeProvider.System);
        var businessId = invoice.BusinessId;

        await service.ExecuteAsync(invoice.Id, businessId);
        var firstCancelledAt = invoice.CancelledAtUtc;
        await service.ExecuteAsync(invoice.Id, businessId);

        Assert.Equal(firstCancelledAt, invoice.CancelledAtUtc);
        Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    private static Invoice CreateInvoice(Guid businessId) =>
        new(businessId, Guid.NewGuid(), null, 17, DateTime.UtcNow,
            [new InvoiceLine(Guid.NewGuid(), "Producto", 1m, 10m)]);

    private sealed class FakeInvoiceRepository(Invoice invoice) : IInvoiceRepository
    {
        public Task<Invoice?> GetByIdAsync(Guid id, Guid businessId, CancellationToken token = default) =>
            Task.FromResult<Invoice?>(invoice.Id == id && invoice.BusinessId == businessId ? invoice : null);
        public Task<IReadOnlyList<Invoice>> GetAllAsync(Guid businessId, CancellationToken token = default) =>
            Task.FromResult<IReadOnlyList<Invoice>>([]);
        public Task<IReadOnlyList<Invoice>> GetPageAsync(Guid businessId, int skip, int take, CancellationToken token = default) =>
            Task.FromResult<IReadOnlyList<Invoice>>([]);
        public Task<long> GetCountAsync(Guid businessId, CancellationToken token = default) => Task.FromResult(0L);
        public Task<long> GetNextNumberAsync(Guid businessId, CancellationToken token = default) => Task.FromResult(18L);
        public Task AddAsync(Invoice value, CancellationToken token = default) => Task.CompletedTask;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }
        public Task<int> SaveChangesAsync(CancellationToken token = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}

