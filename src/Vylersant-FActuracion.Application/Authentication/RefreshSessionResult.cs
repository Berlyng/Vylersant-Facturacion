namespace Vylersant_Facturacion.Application.Authentication;

public sealed record RefreshSessionResult(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);