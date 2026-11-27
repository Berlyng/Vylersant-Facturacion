namespace Vylersant_Facturacion.Desktop.Authentication;

public sealed record SessionTokens(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);

public sealed record AuthenticatedSession(SessionTokens Tokens, Vylersant_Facturacion.Contracts.Authentication.CurrentUserResponse User);

