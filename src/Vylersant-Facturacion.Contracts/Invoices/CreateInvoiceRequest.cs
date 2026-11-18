namespace Vylersant_Facturacion.Contracts.Invoices;

public sealed record CreateInvoiceRequest(
    Guid? CustomerId,
    IReadOnlyList<CreateInvoiceLineRequest> Lines);

public sealed record CreateInvoiceLineRequest(
    Guid ProductId,
    decimal Quantity);

