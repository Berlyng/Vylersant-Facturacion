using Vylersant_Facturacion.Application.Invoices;
using Vylersant_Facturacion.Domain.Entities.Payments;

namespace Vylersant_Facturacion.Application.Payments;

public sealed class GetInvoicePaymentsService(IInvoiceRepository invoiceRepository, IPaymentRepository paymentRepository)
{
    public async Task<InvoicePayments> ExecuteAsync(Guid invoiceId, Guid businessId, CancellationToken cancellationToken = default)
    {
        if (invoiceId == Guid.Empty) throw new ArgumentException("El identificador de la factura es obligatorio.", nameof(invoiceId));
        if (businessId == Guid.Empty) throw new ArgumentException("El identificador del negocio es obligatorio.", nameof(businessId));

        var invoice = await invoiceRepository.GetByIdAsync(invoiceId, businessId, cancellationToken)
            ?? throw new InvoiceNotFoundException(invoiceId);
        var payments = await paymentRepository.GetForInvoiceAsync(invoiceId, businessId, cancellationToken);
        var paidTotal = decimal.Round(payments.Where(payment => payment.Status == PaymentStatus.Completed).Sum(payment => payment.Amount), 2, MidpointRounding.AwayFromZero);
        var balance = decimal.Round(Math.Max(0m, invoice.Total - paidTotal), 2, MidpointRounding.AwayFromZero);
        return new InvoicePayments(invoice.Id, invoice.Total, paidTotal, balance, payments);
    }
}

