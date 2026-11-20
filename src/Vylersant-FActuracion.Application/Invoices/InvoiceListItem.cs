using Vylersant_Facturacion.Domain.Entities.Invoices;

namespace Vylersant_Facturacion.Application.Invoices;

public sealed record InvoiceListItem(
    Guid Id,
    long Number,
    DateTime IssuedAtUtc,
    InvoiceStatus Status,
    Guid? CustomerId,
    decimal Total);

