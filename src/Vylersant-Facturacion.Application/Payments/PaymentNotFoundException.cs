namespace Vylersant_Facturacion.Application.Payments;

public sealed class PaymentNotFoundException(Guid paymentId)
    : Exception($"El pago '{paymentId}' no fue encontrado.");

