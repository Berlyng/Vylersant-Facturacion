using Vylersant_Facturacion.Contracts.Customers;
using Vylersant_Facturacion.Contracts.Products;

namespace Vylersant_Facturacion.Desktop.PointOfSale;

public interface ICatalogApiClient
{
    Task<IReadOnlyList<ProductResponse>> GetProductsAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerResponse>> GetCustomersAsync(string accessToken, CancellationToken cancellationToken = default);
}

