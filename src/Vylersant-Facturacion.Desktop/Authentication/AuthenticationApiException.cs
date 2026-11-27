using System.Net;

namespace Vylersant_Facturacion.Desktop.Authentication;

public sealed class AuthenticationApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}

