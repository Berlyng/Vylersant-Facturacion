using System.Net;

namespace Vylersant_Facturacion.Desktop.PointOfSale;

public sealed class CatalogApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}

