using Vylersant_Facturacion.Contracts.Invoices;

namespace Vylersant_Facturacion.Desktop.PointOfSale;

public interface IInvoiceApiClient
{
    Task<CreateInvoiceResponse> CreateAsync(CreateInvoiceRequest request, string accessToken, CancellationToken cancellationToken = default);
    Task<InvoicePageResponse> GetPageAsync(int page, int pageSize, string accessToken, CancellationToken cancellationToken = default);
    Task<InvoiceDetailsResponse> GetDetailsAsync(Guid invoiceId, string accessToken, CancellationToken cancellationToken = default);
}

