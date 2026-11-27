using System.Net;
using Vylersant_Facturacion.Contracts.Authentication;
using Vylersant_Facturacion.Desktop.Authentication;

namespace Vylersant_Facturacion.Desktop.Tests.Authentication;

public sealed class SessionServiceTests
{
    [Fact]
    public async Task LoginAsync_ShouldStoreOnlySessionTokens()
    {
        var api = new FakeAuthenticationApiClient();
        var store = new FakeTokenStore();
        var service = new SessionService(api, store, new FixedTimeProvider(DateTimeOffset.UtcNow));

        var session = await service.LoginAsync(" owner@example.com ", "secret");

        Assert.Equal("owner@example.com", api.LoginEmail);
        Assert.Equal("secret", api.LoginPassword);
        Assert.Equal("refresh-1", store.Tokens?.RefreshToken);
        Assert.Equal("Owner", session.User.Name);
    }

    [Fact]
    public async Task RestoreAsync_ShouldRefreshExpiredAccessTokenBeforeRequiringSession()
    {
        var now = new DateTimeOffset(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
        var api = new FakeAuthenticationApiClient();
        var store = new FakeTokenStore(new SessionTokens("expired", now.AddMinutes(-1).UtcDateTime, "refresh-1", now.AddDays(1).UtcDateTime));
        var service = new SessionService(api, store, new FixedTimeProvider(now));

        var session = await service.RestoreAsync();

        Assert.NotNull(session);
        Assert.Equal(1, api.RefreshCalls);
        Assert.Equal("access-2", api.LastAccessToken);
        Assert.Equal("refresh-2", store.Tokens?.RefreshToken);
    }

    [Fact]
    public async Task RestoreAsync_ShouldClearSessionWhenRefreshTokenIsRejected()
    {
        var now = new DateTimeOffset(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
        var api = new FakeAuthenticationApiClient { RejectRefresh = true };
        var store = new FakeTokenStore(new SessionTokens("expired", now.AddMinutes(-1).UtcDateTime, "invalid", now.AddDays(1).UtcDateTime));
        var service = new SessionService(api, store, new FixedTimeProvider(now));

        var session = await service.RestoreAsync();

        Assert.Null(session);
        Assert.Null(store.Tokens);
        Assert.Equal(1, store.ClearCalls);
    }

    [Fact]
    public async Task LogoutAsync_ShouldRemovePersistedSession()
    {
        var store = new FakeTokenStore(new SessionTokens("access", DateTime.UtcNow.AddHours(1), "refresh", DateTime.UtcNow.AddDays(1)));
        var service = new SessionService(new FakeAuthenticationApiClient(), store, TimeProvider.System);

        await service.LogoutAsync();

        Assert.Null(store.Tokens);
        Assert.Equal(1, store.ClearCalls);
    }

    private sealed class FakeAuthenticationApiClient : IAuthenticationApiClient
    {
        private static readonly DateTime Expiry = DateTime.UtcNow.AddHours(1);
        public string? LoginEmail { get; private set; }
        public string? LoginPassword { get; private set; }
        public string? LastAccessToken { get; private set; }
        public int RefreshCalls { get; private set; }
        public bool RejectRefresh { get; init; }

        public Task<LoginResponse> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            LoginEmail = email;
            LoginPassword = password;
            return Task.FromResult(new LoginResponse(Guid.NewGuid(), Guid.NewGuid(), "Owner", email, "Owner", "access-1", Expiry, "refresh-1", Expiry.AddDays(7)));
        }

        public Task<RefreshSessionResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            RefreshCalls++;
            if (RejectRefresh)
                throw new AuthenticationApiException(HttpStatusCode.Unauthorized, "Sesión expirada.");
            return Task.FromResult(new RefreshSessionResponse("access-2", Expiry, "refresh-2", Expiry.AddDays(7)));
        }

        public Task<CurrentUserResponse> GetCurrentUserAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            LastAccessToken = accessToken;
            return Task.FromResult(new CurrentUserResponse(Guid.NewGuid(), Guid.NewGuid(), "Owner", "owner@example.com", "Owner"));
        }
    }

    private sealed class FakeTokenStore(SessionTokens? initial = null) : ITokenStore
    {
        public SessionTokens? Tokens { get; private set; } = initial;
        public int ClearCalls { get; private set; }
        public Task<SessionTokens?> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(Tokens);
        public Task SaveAsync(SessionTokens tokens, CancellationToken cancellationToken = default) { Tokens = tokens; return Task.CompletedTask; }
        public Task ClearAsync(CancellationToken cancellationToken = default) { Tokens = null; ClearCalls++; return Task.CompletedTask; }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}

