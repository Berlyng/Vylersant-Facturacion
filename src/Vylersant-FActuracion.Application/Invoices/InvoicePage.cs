namespace Vylersant_Facturacion.Application.Invoices;

public sealed record InvoicePage(
    IReadOnlyList<InvoiceListItem> Items,
    int Page,
    int PageSize,
    long TotalCount);

