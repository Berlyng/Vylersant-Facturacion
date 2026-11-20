using Vylersant_Facturacion.Domain.Entities.Invoices;

namespace Vylersant_Facturacion.Application.Invoices;

public sealed record InvoiceDetails(
    Guid Id,
    long Number,
    DateTime IssuedAtUtc,
    InvoiceStatus Status,
    Guid? CustomerId,
    string? CustomerName,
    decimal Total,
    DateTime? CancelledAtUtc,
    IReadOnlyList<InvoiceLineDetails> Lines);

public sealed record InvoiceLineDetails(
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);

