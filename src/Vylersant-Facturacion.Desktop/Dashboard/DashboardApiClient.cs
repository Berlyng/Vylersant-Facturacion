using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Vylersant_Facturacion.Contracts.Dashboard;

namespace Vylersant_Facturacion.Desktop.Dashboard;

public sealed class DashboardApiClient(HttpClient httpClient) : IDashboardApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<DashboardReportResponse> GetReportAsync(DateTime fromUtc, DateTime toUtc, string groupBy, string accessToken, CancellationToken cancellationToken = default)
    {
        var route = $"api/dashboard?fromUtc={Uri.EscapeDataString(fromUtc.ToUniversalTime().ToString("O"))}" +
                    $"&toUtc={Uri.EscapeDataString(toUtc.ToUniversalTime().ToString("O"))}&groupBy={Uri.EscapeDataString(groupBy)}";
        using var request = new HttpRequestMessage(HttpMethod.Get, route);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<DashboardReportResponse>(JsonOptions, cancellationToken)
                ?? throw new DashboardApiException("El servidor devolvió un informe vacío.");

        var detail = "No se pudo cargar el dashboard.";
        try
        {
            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            if (body.RootElement.TryGetProperty("detail", out var element))
                detail = element.GetString() ?? detail;
        }
        catch (JsonException) { }
        throw new DashboardApiException(detail);
    }
}

public sealed class DashboardApiException(string message) : Exception(message);

