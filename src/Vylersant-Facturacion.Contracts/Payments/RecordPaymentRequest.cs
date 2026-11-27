namespace Vylersant_Facturacion.Contracts.Payments;

public sealed record RecordPaymentRequest(decimal Amount, string Method, string? Reference = null);

