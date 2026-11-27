using System.Windows;
using System.Net.Http;
using Vylersant_Facturacion.Desktop.Authentication;

namespace Vylersant_Facturacion.Desktop;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var baseUrl = Environment.GetEnvironmentVariable("VYLERSANT_API_URL") ?? "https://localhost:7025/";
        var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl, UriKind.Absolute), Timeout = TimeSpan.FromSeconds(20) };
        var authenticationClient = new AuthenticationApiClient(httpClient);
        var session = new SessionService(authenticationClient, new DpapiTokenStore(), TimeProvider.System);
        var window = new MainWindow(session);
        MainWindow = window;
        window.Show();
        await window.RestoreSessionAsync();
    }
}

