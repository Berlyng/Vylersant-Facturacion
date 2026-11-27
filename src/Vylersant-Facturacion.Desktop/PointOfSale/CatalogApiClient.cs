using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Vylersant_Facturacion.Contracts.Customers;
using Vylersant_Facturacion.Contracts.Products;

namespace Vylersant_Facturacion.Desktop.PointOfSale;

public sealed class CatalogApiClient(HttpClient httpClient) : ICatalogApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<IReadOnlyList<ProductResponse>> GetProductsAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetAsync<ProductResponse>("api/products", accessToken, cancellationToken);

    public Task<IReadOnlyList<CustomerResponse>> GetCustomersAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetAsync<CustomerResponse>("api/customers", accessToken, cancellationToken);

    private async Task<IReadOnlyList<T>> GetAsync<T>(string route, string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, route);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<List<T>>(JsonOptions, cancellationToken) ?? [];

        var statusCode = response.StatusCode;
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = statusCode switch
        {
            HttpStatusCode.Unauthorized => "La sesión expiró. Cierra sesión e inicia de nuevo.",
            HttpStatusCode.Forbidden => "Tu usuario no tiene permiso para consultar el catálogo.",
            _ => ExtractDetail(body) ?? "No fue posible cargar el catálogo. Inténtalo de nuevo."
        };
        throw new CatalogApiException(statusCode, message);
    }

    private static string? ExtractDetail(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("detail", out var detail))
                return detail.GetString();
        }
        catch (JsonException)
        {
            // Non-ProblemDetails responses use the generic message.
        }
        return null;
    }
}

