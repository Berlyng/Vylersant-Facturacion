using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Invoices;
using Vylersant_Facturacion.Application.Payments;
using Vylersant_Facturacion.Domain.Entities.Invoices;
using Vylersant_Facturacion.Domain.Entities.Payments;

namespace Vylersant_Facturacion.Application.Tests.Payments;

public sealed class RecordPaymentServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldRecordPaymentAndReturnDerivedBalance()
    {
        var invoice = CreateInvoice();
        var payments = new FakePaymentRepository();
        var service = CreateService(invoice, payments, new FakeUnitOfWork());

        var result = await service.ExecuteAsync(invoice.Id, invoice.BusinessId,
            new RecordPaymentRequest(4m, PaymentMethod.Cash));

        Assert.Equal(4m, result.PaidTotal);
        Assert.Equal(6m, result.Balance);
        Assert.False(result.IsPaid);
        Assert.Single(payments.Items);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRejectOverpaymentAndNotPersist()
    {
        var invoice = CreateInvoice();
        var payments = new FakePaymentRepository();
        var service = CreateService(invoice, payments, new FakeUnitOfWork());

        await Assert.ThrowsAsync<InvoicePaymentException>(() => service.ExecuteAsync(
            invoice.Id, invoice.BusinessId, new RecordPaymentRequest(10.01m, PaymentMethod.Card)));

        Assert.Empty(payments.Items);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReportFullyPaidFromDerivedBalance()
    {
        var invoice = CreateInvoice();
        var payments = new FakePaymentRepository();
        var service = CreateService(invoice, payments, new FakeUnitOfWork());

        var result = await service.ExecuteAsync(invoice.Id, invoice.BusinessId,
            new RecordPaymentRequest(10m, PaymentMethod.Transfer));

        Assert.Equal(0m, result.Balance);
        Assert.True(result.IsPaid);
    }

    private static Invoice CreateInvoice() => new(Guid.NewGuid(), Guid.NewGuid(), null, 1, DateTime.UtcNow,
        [new InvoiceLine(Guid.NewGuid(), "Item", 1m, 10m)]);

    private static RecordPaymentService CreateService(Invoice invoice, FakePaymentRepository payments, FakeUnitOfWork unitOfWork) =>
        new(new FakeInvoiceRepository(invoice), payments, unitOfWork, TimeProvider.System);

    private sealed class FakeInvoiceRepository(Invoice invoice) : IInvoiceRepository
    {
        public Task<Invoice?> GetByIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken = default) =>
            Task.FromResult<Invoice?>(invoice.Id == id && invoice.BusinessId == businessId ? invoice : null);
        public Task<IReadOnlyList<Invoice>> GetAllAsync(Guid businessId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Invoice>>([]);
        public Task<IReadOnlyList<Invoice>> GetPageAsync(Guid businessId, int skip, int take, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Invoice>>([]);
        public Task<long> GetCountAsync(Guid businessId, CancellationToken cancellationToken = default) => Task.FromResult(0L);
        public Task<long> GetNextNumberAsync(Guid businessId, CancellationToken cancellationToken = default) => Task.FromResult(1L);
        public Task AddAsync(Invoice value, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakePaymentRepository : IPaymentRepository
    {
        public List<Payment> Items { get; } = [];
        public Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            Items.Add(payment);
            return Task.CompletedTask;
        }
        public Task<decimal> GetCompletedTotalAsync(Guid invoiceId, Guid businessId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.Where(p => p.InvoiceId == invoiceId && p.BusinessId == businessId && p.Status == PaymentStatus.Completed).Sum(p => p.Amount));
        public Task<Payment?> GetByIdAsync(Guid paymentId, Guid businessId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.FirstOrDefault(p => p.Id == paymentId && p.BusinessId == businessId));
        public Task<IReadOnlyList<Payment>> GetForInvoiceAsync(Guid invoiceId, Guid businessId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Payment>>(Items.Where(p => p.InvoiceId == invoiceId && p.BusinessId == businessId).ToList());
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }
}

