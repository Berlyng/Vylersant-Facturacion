namespace Vylersant_Facturacion.Contracts.Authentication;

public sealed record RefreshSessionRequest(
    string RefreshToken);