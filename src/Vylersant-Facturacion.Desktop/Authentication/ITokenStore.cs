namespace Vylersant_Facturacion.Desktop.Authentication;

public interface ITokenStore
{
    Task<SessionTokens?> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(SessionTokens tokens, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}

