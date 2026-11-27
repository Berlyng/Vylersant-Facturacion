namespace Vylersant_Facturacion.Contracts.Payments;

public sealed record PaymentResponse(Guid Id, decimal Amount, DateTime PaidAtUtc, string Method, string? Reference, decimal PaidTotal, decimal Balance, bool IsPaid);

