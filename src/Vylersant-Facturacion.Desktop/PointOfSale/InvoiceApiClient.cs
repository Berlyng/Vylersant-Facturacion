using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Vylersant_Facturacion.Contracts.Invoices;

namespace Vylersant_Facturacion.Desktop.PointOfSale;

public sealed class InvoiceApiClient(HttpClient httpClient) : IInvoiceApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<CreateInvoiceResponse> CreateAsync(CreateInvoiceRequest request, string accessToken, CancellationToken cancellationToken = default)
    {
        using var message = CreateRequest(HttpMethod.Post, "api/invoices", accessToken);
        message.Content = JsonContent.Create(request, options: JsonOptions);
        using var response = await httpClient.SendAsync(message, cancellationToken);
        return await ReadResponseAsync<CreateInvoiceResponse>(response, cancellationToken);
    }

    public async Task<InvoicePageResponse> GetPageAsync(int page, int pageSize, string accessToken, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.SendAsync(
            CreateRequest(HttpMethod.Get, $"api/invoices?page={page}&pageSize={pageSize}", accessToken), cancellationToken);
        return await ReadResponseAsync<InvoicePageResponse>(response, cancellationToken);
    }

    public async Task<InvoiceDetailsResponse> GetDetailsAsync(Guid invoiceId, string accessToken, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.SendAsync(
            CreateRequest(HttpMethod.Get, $"api/invoices/{invoiceId}", accessToken), cancellationToken);
        return await ReadResponseAsync<InvoiceDetailsResponse>(response, cancellationToken);
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string route, string accessToken)
    {
        var request = new HttpRequestMessage(method, route);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
                ?? throw new InvoiceApiException(response.StatusCode, "El servidor devolvió una respuesta vacía.");

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "La sesión expiró. Inicia sesión de nuevo.",
            HttpStatusCode.Forbidden => "Tu usuario no tiene permiso para consultar estas facturas.",
            HttpStatusCode.NotFound => "La factura solicitada ya no está disponible.",
            _ => ExtractDetail(body) ?? "No fue posible completar la operación de facturación."
        };
        throw new InvoiceApiException(response.StatusCode, message);
    }

    private static string? ExtractDetail(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("detail", out var detail))
                return detail.GetString();
            if (document.RootElement.TryGetProperty("title", out var title))
                return title.GetString();
        }
        catch (JsonException)
        {
            // Non-ProblemDetails responses use the generic message.
        }

        return null;
    }
}

