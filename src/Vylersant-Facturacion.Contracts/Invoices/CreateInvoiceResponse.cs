namespace Vylersant_Facturacion.Contracts.Invoices;

public sealed record CreateInvoiceResponse(
    Guid InvoiceId,
    long Number,
    decimal Total);

