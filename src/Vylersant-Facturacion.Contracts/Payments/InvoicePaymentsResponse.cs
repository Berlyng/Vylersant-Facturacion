namespace Vylersant_Facturacion.Contracts.Payments;

public sealed record InvoicePaymentsResponse(Guid InvoiceId, decimal InvoiceTotal, decimal PaidTotal, decimal Balance, bool IsPaid, IReadOnlyList<PaymentHistoryItemResponse> Payments);

public sealed record PaymentHistoryItemResponse(Guid Id, decimal Amount, DateTime PaidAtUtc, string Method, string Status, string? Reference, DateTime? VoidedAtUtc, string? VoidReason);

