namespace Vylersant_Facturacion.Application.Authentication;

public sealed record RefreshSessionRequest(
    string RefreshToken);