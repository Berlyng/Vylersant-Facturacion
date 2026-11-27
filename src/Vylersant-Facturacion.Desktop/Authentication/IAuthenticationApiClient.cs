using Vylersant_Facturacion.Contracts.Authentication;

namespace Vylersant_Facturacion.Desktop.Authentication;

public interface IAuthenticationApiClient
{
    Task<LoginResponse> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<RefreshSessionResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<CurrentUserResponse> GetCurrentUserAsync(string accessToken, CancellationToken cancellationToken = default);
}

