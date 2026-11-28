using System.Windows;
using System.Net.Http;
using Vylersant_Facturacion.Desktop.Authentication;
using Vylersant_Facturacion.Desktop.PointOfSale;
using Vylersant_Facturacion.Desktop.Printing;
using Vylersant_Facturacion.Desktop.Dashboard;

namespace Vylersant_Facturacion.Desktop;

public partial class App : System.Windows.Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var baseUrl = Environment.GetEnvironmentVariable("VYLERSANT_API_URL") ?? "https://localhost:7025/";
        var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl, UriKind.Absolute), Timeout = TimeSpan.FromSeconds(20) };
        var authenticationClient = new AuthenticationApiClient(httpClient);
        var session = new SessionService(authenticationClient, new DpapiTokenStore(), TimeProvider.System);
        var catalogClient = new CatalogApiClient(httpClient);
        var invoiceClient = new InvoiceApiClient(httpClient);
        var dashboardClient = new DashboardApiClient(httpClient);
        var printerSettingsStore = new ReceiptPrinterSettingsStore();
        var receiptPrinter = new EscPosReceiptPrinter(new WindowsRawPrinterTransport(), TimeProvider.System);
        var window = new MainWindow(session, catalogClient, invoiceClient, printerSettingsStore, receiptPrinter, dashboardClient);
        MainWindow = window;
        window.Show();
        await window.RestoreSessionAsync();
    }
}

