using Vylersant_Facturacion.Domain.Entities.Payments;

namespace Vylersant_Facturacion.Application.Payments;

public sealed record RecordPaymentRequest(decimal Amount, PaymentMethod Method, string? Reference = null);

