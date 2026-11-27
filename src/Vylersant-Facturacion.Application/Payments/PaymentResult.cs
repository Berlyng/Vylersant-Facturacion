using Vylersant_Facturacion.Domain.Entities.Payments;

namespace Vylersant_Facturacion.Application.Payments;

public sealed record PaymentResult(Guid Id, decimal Amount, DateTime PaidAtUtc, PaymentMethod Method, string? Reference, decimal PaidTotal, decimal Balance)
{
    public bool IsPaid => Balance == 0m;
}

