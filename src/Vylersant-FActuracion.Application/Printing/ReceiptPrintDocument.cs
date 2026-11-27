namespace Vylersant_Facturacion.Application.Printing;

/// <summary>Immutable invoice snapshot formatted as input for a receipt printer.</summary>
public sealed record ReceiptPrintDocument(
    long InvoiceNumber,
    DateTime IssuedAtUtc,
    string? CustomerName,
    decimal Total,
    IReadOnlyList<ReceiptPrintLine> Lines);

public sealed record ReceiptPrintLine(
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);

