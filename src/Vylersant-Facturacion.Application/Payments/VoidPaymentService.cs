using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Invoices;
using Vylersant_Facturacion.Domain.Entities.Payments;

namespace Vylersant_Facturacion.Application.Payments;

public sealed class VoidPaymentService(
    IInvoiceRepository invoiceRepository,
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task ExecuteAsync(Guid invoiceId, Guid paymentId, Guid businessId, string reason, CancellationToken cancellationToken = default)
    {
        if (invoiceId == Guid.Empty) throw new ArgumentException("El identificador de la factura es obligatorio.", nameof(invoiceId));
        if (paymentId == Guid.Empty) throw new ArgumentException("El identificador del pago es obligatorio.", nameof(paymentId));
        if (businessId == Guid.Empty) throw new ArgumentException("El identificador del negocio es obligatorio.", nameof(businessId));
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("El motivo de anulación es obligatorio.", nameof(reason));

        _ = await invoiceRepository.GetByIdAsync(invoiceId, businessId, cancellationToken)
            ?? throw new InvoiceNotFoundException(invoiceId);
        var payment = await paymentRepository.GetByIdAsync(paymentId, businessId, cancellationToken)
            ?? throw new PaymentNotFoundException(paymentId);
        if (payment.InvoiceId != invoiceId)
            throw new PaymentNotFoundException(paymentId);
        if (payment.Status == PaymentStatus.Voided)
            return;

        payment.Void(timeProvider.GetUtcNow().UtcDateTime, reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

