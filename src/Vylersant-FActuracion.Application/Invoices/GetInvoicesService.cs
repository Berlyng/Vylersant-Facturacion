namespace Vylersant_Facturacion.Application.Invoices;

public sealed class GetInvoicesService
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoicesService(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<InvoicePage> ExecuteAsync(
        Guid businessId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
        {
            throw new ArgumentException("El identificador del negocio es obligatorio.", nameof(businessId));
        }

        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "La página debe ser mayor que cero.");
        }

        if (pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "El tamaño de página debe estar entre 1 y 100.");
        }

        if (page - 1 > int.MaxValue / pageSize)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "La página solicitada está fuera del rango permitido.");
        }

        var skip = (page - 1) * pageSize;
        var invoices = await _invoiceRepository.GetPageAsync(businessId, skip, pageSize, cancellationToken);
        var totalCount = await _invoiceRepository.GetCountAsync(businessId, cancellationToken);

        return new InvoicePage(
            invoices.Select(invoice => new InvoiceListItem(
                invoice.Id,
                invoice.Number,
                invoice.IssuedAtUtc,
                invoice.Status,
                invoice.CustomerId,
                invoice.Total)).ToList(),
            page,
            pageSize,
            totalCount);
    }
}

