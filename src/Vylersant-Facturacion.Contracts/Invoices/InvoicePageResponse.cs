namespace Vylersant_Facturacion.Contracts.Invoices;

public sealed record InvoicePageResponse(
    IReadOnlyList<InvoiceListItemResponse> Items,
    int Page,
    int PageSize,
    long TotalCount);

public sealed record InvoiceListItemResponse(
    Guid Id,
    long Number,
    DateTime IssuedAtUtc,
    string Status,
    Guid? CustomerId,
    decimal Total);

