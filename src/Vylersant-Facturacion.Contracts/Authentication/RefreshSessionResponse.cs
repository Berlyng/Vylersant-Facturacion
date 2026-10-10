namespace Vylersant_Facturacion.Contracts.Authentication;

public sealed record RefreshSessionResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);