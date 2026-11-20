namespace Vylersant_Facturacion.Contracts.Invoices;

public sealed record InvoiceDetailsResponse(
    Guid Id,
    long Number,
    DateTime IssuedAtUtc,
    string Status,
    Guid? CustomerId,
    string? CustomerName,
    decimal Total,
    DateTime? CancelledAtUtc,
    IReadOnlyList<InvoiceLineResponse> Lines);

public sealed record InvoiceLineResponse(
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);

