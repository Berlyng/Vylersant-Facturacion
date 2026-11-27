using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Invoices;
using Vylersant_Facturacion.Application.Payments;
using Vylersant_Facturacion.Domain.Entities.Invoices;
using Vylersant_Facturacion.Domain.Entities.Payments;

namespace Vylersant_Facturacion.Application.Tests.Payments;

public sealed class PaymentHistoryAndVoidTests
{
    [Fact]
    public async Task Void_ShouldBeIdempotentAndRecalculateBalance()
    {
        var invoice = MakeInvoice();
        var payment = new Payment(invoice.BusinessId, invoice.Id, 4m, DateTime.UtcNow, PaymentMethod.Cash);
        var payments = new FakePaymentRepository(payment);
        var unitOfWork = new FakeUnitOfWork();
        var fixedTime = new FixedTimeProvider(new DateTimeOffset(2026, 11, 26, 12, 0, 0, TimeSpan.Zero));
        var service = new VoidPaymentService(new FakeInvoiceRepository(invoice), payments, unitOfWork, fixedTime);

        await service.ExecuteAsync(invoice.Id, payment.Id, invoice.BusinessId, "Pago duplicado");
        await service.ExecuteAsync(invoice.Id, payment.Id, invoice.BusinessId, "Otro motivo");
        var history = await new GetInvoicePaymentsService(new FakeInvoiceRepository(invoice), payments)
            .ExecuteAsync(invoice.Id, invoice.BusinessId);

        Assert.Equal(PaymentStatus.Voided, payment.Status);
        Assert.Equal("Pago duplicado", payment.VoidReason);
        Assert.Equal(fixedTime.GetUtcNow().UtcDateTime, payment.VoidedAtUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
        Assert.Equal(0m, history.PaidTotal);
        Assert.Equal(invoice.Total, history.Balance);
        Assert.Single(history.Payments);
    }

    [Fact]
    public async Task Void_ShouldNotExposePaymentFromAnotherBusiness()
    {
        var invoice = MakeInvoice();
        var payment = new Payment(invoice.BusinessId, invoice.Id, 4m, DateTime.UtcNow, PaymentMethod.Cash);
        var service = new VoidPaymentService(new FakeInvoiceRepository(invoice), new FakePaymentRepository(payment), new FakeUnitOfWork(), TimeProvider.System);

        await Assert.ThrowsAsync<PaymentNotFoundException>(() => service.ExecuteAsync(
            invoice.Id, payment.Id, Guid.NewGuid(), "Ajuste"));

        Assert.Equal(PaymentStatus.Completed, payment.Status);
    }

    private static Invoice MakeInvoice() => new(Guid.NewGuid(), Guid.NewGuid(), null, 1, DateTime.UtcNow,
        [new InvoiceLine(Guid.NewGuid(), "Producto", 1m, 10m)]);

    private sealed class FakeInvoiceRepository(Invoice invoice) : IInvoiceRepository
    {
        public Task<Invoice?> GetByIdAsync(Guid id, Guid businessId, CancellationToken token = default) =>
            Task.FromResult<Invoice?>(invoice.Id == id && invoice.BusinessId == businessId ? invoice : null);
        public Task<IReadOnlyList<Invoice>> GetAllAsync(Guid businessId, CancellationToken token = default) => Task.FromResult<IReadOnlyList<Invoice>>([]);
        public Task<IReadOnlyList<Invoice>> GetPageAsync(Guid businessId, int skip, int take, CancellationToken token = default) => Task.FromResult<IReadOnlyList<Invoice>>([]);
        public Task<long> GetCountAsync(Guid businessId, CancellationToken token = default) => Task.FromResult(0L);
        public Task<long> GetNextNumberAsync(Guid businessId, CancellationToken token = default) => Task.FromResult(1L);
        public Task AddAsync(Invoice value, CancellationToken token = default) => Task.CompletedTask;
    }

    private sealed class FakePaymentRepository(params Payment[] payments) : IPaymentRepository
    {
        private readonly List<Payment> _payments = [.. payments];
        public Task AddAsync(Payment payment, CancellationToken cancellationToken = default) { _payments.Add(payment); return Task.CompletedTask; }
        public Task<Payment?> GetByIdAsync(Guid paymentId, Guid businessId, CancellationToken cancellationToken = default) => Task.FromResult(_payments.FirstOrDefault(p => p.Id == paymentId && p.BusinessId == businessId));
        public Task<IReadOnlyList<Payment>> GetForInvoiceAsync(Guid invoiceId, Guid businessId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Payment>>(_payments.Where(p => p.InvoiceId == invoiceId && p.BusinessId == businessId).ToList());
        public Task<decimal> GetCompletedTotalAsync(Guid invoiceId, Guid businessId, CancellationToken cancellationToken = default) => Task.FromResult(_payments.Where(p => p.InvoiceId == invoiceId && p.BusinessId == businessId && p.Status == PaymentStatus.Completed).Sum(p => p.Amount));
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) { SaveCount++; return Task.FromResult(1); }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}

