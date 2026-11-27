using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Vylersant_Facturacion.Contracts.Authentication;
using LoginRequest = Vylersant_Facturacion.Contracts.Authentication.LoginRequest;

namespace Vylersant_Facturacion.Desktop.Authentication;

public sealed class AuthenticationApiClient(HttpClient httpClient) : IAuthenticationApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<LoginResponse> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/auth/login", new LoginRequest(email, password), JsonOptions, cancellationToken);
        return await ReadRequiredResponseAsync<LoginResponse>(response, cancellationToken);
    }

    public async Task<RefreshSessionResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/auth/refresh", new RefreshSessionRequest(refreshToken), JsonOptions, cancellationToken);
        return await ReadRequiredResponseAsync<RefreshSessionResponse>(response, cancellationToken);
    }

    public async Task<CurrentUserResponse> GetCurrentUserAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        return await ReadRequiredResponseAsync<CurrentUserResponse>(response, cancellationToken);
    }

    private static async Task<T> ReadRequiredResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
                ?? throw new AuthenticationApiException(response.StatusCode, "El servidor devolvió una respuesta vacía.");
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "El correo o la contraseña son incorrectos, o la sesión expiró.",
            HttpStatusCode.Forbidden => "Tu usuario no tiene permiso para acceder.",
            _ => ExtractProblemMessage(body) ?? "No fue posible completar la solicitud de autenticación."
        };
        throw new AuthenticationApiException(response.StatusCode, message);
    }

    private static string? ExtractProblemMessage(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.TryGetProperty("detail", out var detail) && !string.IsNullOrWhiteSpace(detail.GetString()))
                return detail.GetString();
            if (root.TryGetProperty("title", out var title))
                return title.GetString();
        }
        catch (JsonException)
        {
            // A non-ProblemDetails response is shown through the generic message.
        }

        return null;
    }
}

