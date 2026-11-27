using System.Net;
using Vylersant_Facturacion.Contracts.Authentication;

namespace Vylersant_Facturacion.Desktop.Authentication;

public sealed class SessionService(IAuthenticationApiClient api, ITokenStore tokenStore, TimeProvider timeProvider)
{
    public async Task<AuthenticatedSession> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Escribe tu correo y contraseña.");

        var result = await api.LoginAsync(email.Trim(), password, cancellationToken);
        var tokens = new SessionTokens(result.AccessToken, result.AccessTokenExpiresAtUtc, result.RefreshToken, result.RefreshTokenExpiresAtUtc);
        await tokenStore.SaveAsync(tokens, cancellationToken);
        return new AuthenticatedSession(tokens, new CurrentUserResponse(result.UserId, result.BusinessId, result.Name, result.Email, result.Role));
    }

    public async Task<AuthenticatedSession?> RestoreAsync(CancellationToken cancellationToken = default)
    {
        var tokens = await tokenStore.LoadAsync(cancellationToken);
        if (tokens is null)
            return null;

        try
        {
            if (tokens.AccessTokenExpiresAtUtc <= timeProvider.GetUtcNow().UtcDateTime.AddSeconds(30))
                tokens = await RefreshAsync(tokens, cancellationToken);

            try
            {
                var user = await api.GetCurrentUserAsync(tokens.AccessToken, cancellationToken);
                return new AuthenticatedSession(tokens, user);
            }
            catch (AuthenticationApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                tokens = await RefreshAsync(tokens, cancellationToken);
                var user = await api.GetCurrentUserAsync(tokens.AccessToken, cancellationToken);
                return new AuthenticatedSession(tokens, user);
            }
        }
        catch (AuthenticationApiException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            await tokenStore.ClearAsync(cancellationToken);
            return null;
        }
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default) => tokenStore.ClearAsync(cancellationToken);

    private async Task<SessionTokens> RefreshAsync(SessionTokens tokens, CancellationToken cancellationToken)
    {
        if (tokens.RefreshTokenExpiresAtUtc <= timeProvider.GetUtcNow().UtcDateTime)
            throw new AuthenticationApiException(HttpStatusCode.Unauthorized, "La sesión expiró. Inicia sesión de nuevo.");

        var result = await api.RefreshAsync(tokens.RefreshToken, cancellationToken);
        var refreshed = new SessionTokens(result.AccessToken, result.AccessTokenExpiresAtUtc, result.RefreshToken, result.RefreshTokenExpiresAtUtc);
        await tokenStore.SaveAsync(refreshed, cancellationToken);
        return refreshed;
    }
}

