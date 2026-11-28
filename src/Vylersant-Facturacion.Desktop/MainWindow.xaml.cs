using System.Net.Http;
using System.Windows;
using Vylersant_Facturacion.Desktop.Authentication;
using Vylersant_Facturacion.Desktop.PointOfSale;
using Vylersant_Facturacion.Contracts.Products;
using Vylersant_Facturacion.Contracts.Invoices;
using Vylersant_Facturacion.Application.Printing;
using Vylersant_Facturacion.Desktop.Printing;

namespace Vylersant_Facturacion.Desktop;

public partial class MainWindow : Window
{
    private readonly SessionService _sessionService;
    private readonly PosViewModel _posViewModel;
    private readonly InvoiceHistoryViewModel _invoiceHistoryViewModel;
    private readonly ReceiptPrinterSettingsStore _printerSettingsStore;
    private readonly EscPosReceiptPrinter _receiptPrinter;
    private AuthenticatedSession? _currentSession;

    public MainWindow(SessionService sessionService, ICatalogApiClient catalogApiClient, IInvoiceApiClient invoiceApiClient,
        ReceiptPrinterSettingsStore printerSettingsStore, EscPosReceiptPrinter receiptPrinter)
    {
        _sessionService = sessionService;
        _posViewModel = new PosViewModel(catalogApiClient, invoiceApiClient);
        _invoiceHistoryViewModel = new InvoiceHistoryViewModel(invoiceApiClient);
        _printerSettingsStore = printerSettingsStore;
        _receiptPrinter = receiptPrinter;
        InitializeComponent();
        LoginPanel.Visibility = Visibility.Visible;
        PosContent.DataContext = _posViewModel;
        InvoicesContent.DataContext = _invoiceHistoryViewModel;
        UpdateWindowStateButton();
    }

    public async Task RestoreSessionAsync()
    {
        try
        {
            var session = await _sessionService.RestoreAsync();
            if (session is not null)
                ShowAuthenticatedShell(session);
        }
        catch (HttpRequestException)
        {
            LoginErrorText.Text = "No se pudo conectar con el servidor. Verifica la conexión e inténtalo de nuevo.";
        }
        catch (TaskCanceledException)
        {
            LoginErrorText.Text = "El servidor tardó demasiado en responder. Inténtalo de nuevo.";
        }
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        LoginErrorText.Text = string.Empty;
        LoginButton.IsEnabled = false;
        try
        {
            var session = await _sessionService.LoginAsync(EmailTextBox.Text, PasswordBox.Password);
            PasswordBox.Clear();
            ShowAuthenticatedShell(session);
        }
        catch (AuthenticationApiException exception)
        {
            LoginErrorText.Text = exception.Message;
        }
        catch (HttpRequestException)
        {
            LoginErrorText.Text = "No se pudo conectar con el servidor. Verifica la conexión e inténtalo de nuevo.";
        }
        catch (TaskCanceledException)
        {
            LoginErrorText.Text = "El servidor tardó demasiado en responder. Inténtalo de nuevo.";
        }
        finally
        {
            LoginButton.IsEnabled = true;
        }
    }

    private async void Logout_Click(object sender, RoutedEventArgs e)
    {
        await _sessionService.LogoutAsync();
        _currentSession = null;
        _posViewModel.Clear();
        ShellPanel.Visibility = Visibility.Collapsed;
        LoginPanel.Visibility = Visibility.Visible;
        EmailTextBox.Clear();
        PasswordBox.Clear();
        LoginErrorText.Text = "La sesión se cerró en este dispositivo.";
    }

    private async void Navigation_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string section })
            return;

        PageTitleText.Text = section;
        PageDescriptionText.Text = section == "Inicio"
            ? "Tu espacio de trabajo está listo."
            : section == "POS"
                ? "Busca productos y prepara el carrito de venta."
                : section == "Facturas"
                    ? "Consulta las ventas y revisa el detalle guardado por el servidor."
                    : $"La sección de {section.ToLowerInvariant()} estará disponible en un siguiente issue.";
        var showPos = section == "POS";
        var showInvoices = section == "Facturas";
        PosContent.Visibility = showPos ? Visibility.Visible : Visibility.Collapsed;
        InvoicesContent.Visibility = showInvoices ? Visibility.Visible : Visibility.Collapsed;
        HomeContent.Visibility = showPos || showInvoices ? Visibility.Collapsed : Visibility.Visible;
        if (showPos && _currentSession is not null)
            await _posViewModel.LoadAsync(_currentSession.Tokens.AccessToken);
        else if (showInvoices && _currentSession is not null)
            await _invoiceHistoryViewModel.LoadAsync(_currentSession.Tokens.AccessToken);
    }

    private async void ConfirmSale_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSession is null)
            return;
        await _posViewModel.ConfirmSaleAsync(_currentSession.Tokens.AccessToken);
    }

    private async void InvoiceItem_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSession is null || sender is not FrameworkElement { Tag: InvoiceListItemResponse invoice })
            return;
        await _invoiceHistoryViewModel.LoadDetailsAsync(invoice, _currentSession.Tokens.AccessToken);
    }

    private void PrinterSettings_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSession is null)
            return;
        var dialog = new PrinterSettingsWindow(_currentSession.User.BusinessId, _printerSettingsStore)
        {
            Owner = this
        };
        dialog.ShowDialog();
    }

    private async void PrintInvoice_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSession is null || _invoiceHistoryViewModel.Details is not { } details)
            return;

        var businessId = _currentSession.User.BusinessId;
        var settings = await _printerSettingsStore.LoadAsync(businessId);
        if (settings is null)
        {
            PrinterSettings_Click(sender, e);
            settings = await _printerSettingsStore.LoadAsync(businessId);
            if (settings is null)
                return;
        }

        var document = new ReceiptPrintDocument(
            details.Number,
            details.IssuedAtUtc,
            details.CustomerName,
            details.Total,
            details.Lines.Select(line => new ReceiptPrintLine(line.ProductName, line.Quantity, line.UnitPrice, line.LineTotal)).ToList());
        var result = await _receiptPrinter.PrintAsync(document, settings);
        MessageBox.Show(this,
            result.Succeeded ? $"Recibo enviado a la impresora. Trabajo #{result.JobId}." : result.ErrorMessage ?? "No se pudo imprimir el recibo.",
            result.Succeeded ? "Impresión enviada" : "Error de impresión",
            MessageBoxButton.OK,
            result.Succeeded ? MessageBoxImage.Information : MessageBoxImage.Warning);
    }

    private void MinimizeWindow_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void ToggleWindowState_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void CloseWindow_Click(object sender, RoutedEventArgs e) => Close();

    private void Window_StateChanged(object? sender, EventArgs e) => UpdateWindowStateButton();

    private void UpdateWindowStateButton()
    {
        if (WindowStateIcon is null || WindowStateButton is null)
            return;
        var maximized = WindowState == WindowState.Maximized;
        WindowStateIcon.Text = maximized ? "❐" : "□";
        WindowStateButton.ToolTip = maximized ? "Restaurar" : "Maximizar";
        System.Windows.Automation.AutomationProperties.SetName(WindowStateButton, maximized ? "Restaurar" : "Maximizar");
    }

    private void ProductSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) =>
        _posViewModel.SearchText = ProductSearchBox.Text;

    private void AddProduct_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: ProductResponse product })
            _posViewModel.AddProduct(product);
    }

    private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: CartLineViewModel line })
            _posViewModel.ChangeQuantity(line, 1m);
    }

    private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: CartLineViewModel line })
            _posViewModel.ChangeQuantity(line, -1m);
    }

    private void RemoveProduct_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: CartLineViewModel line })
            _posViewModel.RemoveProduct(line);
    }

    private void ShowAuthenticatedShell(AuthenticatedSession session)
    {
        _currentSession = session;
        PosContent.Visibility = Visibility.Collapsed;
        InvoicesContent.Visibility = Visibility.Collapsed;
        HomeContent.Visibility = Visibility.Visible;
        PageTitleText.Text = "Inicio";
        PageDescriptionText.Text = "Tu espacio de trabajo está listo.";
        UserNameText.Text = session.User.Name;
        UserEmailText.Text = session.User.Email;
        LoginPanel.Visibility = Visibility.Collapsed;
        ShellPanel.Visibility = Visibility.Visible;
    }
}

