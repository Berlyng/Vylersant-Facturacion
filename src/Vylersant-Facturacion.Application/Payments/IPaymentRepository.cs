using Vylersant_Facturacion.Domain.Entities.Payments;

namespace Vylersant_Facturacion.Application.Payments;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);

    Task<Payment?> GetByIdAsync(Guid paymentId, Guid businessId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Payment>> GetForInvoiceAsync(Guid invoiceId, Guid businessId, CancellationToken cancellationToken = default);

    Task<decimal> GetCompletedTotalAsync(Guid invoiceId, Guid businessId, CancellationToken cancellationToken = default);
}

