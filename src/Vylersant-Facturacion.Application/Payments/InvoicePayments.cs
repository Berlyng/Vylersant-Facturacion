using Vylersant_Facturacion.Domain.Entities.Payments;

namespace Vylersant_Facturacion.Application.Payments;

public sealed record InvoicePayments(Guid InvoiceId, decimal InvoiceTotal, decimal PaidTotal, decimal Balance, IReadOnlyList<Payment> Payments)
{
    public bool IsPaid => Balance == 0m;
}

