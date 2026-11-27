using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Invoices;
using Vylersant_Facturacion.Domain.Entities.Invoices;
using Vylersant_Facturacion.Domain.Entities.Payments;

namespace Vylersant_Facturacion.Application.Payments;

public sealed class RecordPaymentService(
    IInvoiceRepository invoiceRepository,
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task<PaymentResult> ExecuteAsync(Guid invoiceId, Guid businessId, RecordPaymentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (invoiceId == Guid.Empty) throw new ArgumentException("El identificador de la factura es obligatorio.", nameof(invoiceId));
        if (businessId == Guid.Empty) throw new ArgumentException("El identificador del negocio es obligatorio.", nameof(businessId));

        // Keep the balance check and insertion in one database transaction to avoid concurrent overpayment.
        return await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var invoice = await invoiceRepository.GetByIdAsync(invoiceId, businessId, token)
                ?? throw new InvoiceNotFoundException(invoiceId);
            if (invoice.Status != InvoiceStatus.Issued)
                throw new InvoicePaymentException("Solo se pueden registrar pagos para facturas emitidas.");

            var paidTotal = await paymentRepository.GetCompletedTotalAsync(invoiceId, businessId, token);
            var payment = new Payment(businessId, invoiceId, request.Amount, timeProvider.GetUtcNow().UtcDateTime, request.Method, request.Reference);
            var balance = decimal.Round(invoice.Total - paidTotal, 2, MidpointRounding.AwayFromZero);
            if (payment.Amount > balance)
                throw new InvoicePaymentException("El importe supera el balance pendiente de la factura.");

            await paymentRepository.AddAsync(payment, token);
            await unitOfWork.SaveChangesAsync(token);
            paidTotal = decimal.Round(paidTotal + payment.Amount, 2, MidpointRounding.AwayFromZero);
            return new PaymentResult(payment.Id, payment.Amount, payment.PaidAtUtc, payment.Method, payment.Reference, paidTotal, decimal.Round(invoice.Total - paidTotal, 2, MidpointRounding.AwayFromZero));
        }, System.Data.IsolationLevel.Serializable, cancellationToken);
    }
}

