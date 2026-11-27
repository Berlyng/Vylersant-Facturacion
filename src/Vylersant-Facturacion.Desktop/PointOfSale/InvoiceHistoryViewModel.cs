using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Vylersant_Facturacion.Contracts.Invoices;

namespace Vylersant_Facturacion.Desktop.PointOfSale;

public sealed class InvoiceHistoryViewModel(IInvoiceApiClient invoiceApiClient) : INotifyPropertyChanged
{
    private bool _isLoading;
    private bool _isLoadingDetails;
    private string? _errorMessage;
    private string? _detailsErrorMessage;
    private InvoiceListItemResponse? _selectedInvoice;
    private InvoiceDetailsResponse? _details;
    private long _totalCount;

    public ObservableCollection<InvoiceListItemResponse> Invoices { get; } = [];

    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoInvoices)); }
    }

    public bool IsLoadingDetails
    {
        get => _isLoadingDetails;
        private set { _isLoadingDetails = value; OnPropertyChanged(); }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set { _errorMessage = value; OnPropertyChanged(); }
    }

    public string? DetailsErrorMessage
    {
        get => _detailsErrorMessage;
        private set { _detailsErrorMessage = value; OnPropertyChanged(); }
    }

    public InvoiceListItemResponse? SelectedInvoice
    {
        get => _selectedInvoice;
        private set { _selectedInvoice = value; OnPropertyChanged(); }
    }

    public InvoiceDetailsResponse? Details
    {
        get => _details;
        private set { _details = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasDetails)); }
    }

    public bool HasDetails => Details is not null;
    public bool HasNoInvoices => !IsLoading && string.IsNullOrWhiteSpace(ErrorMessage) && Invoices.Count == 0;
    public long TotalCount
    {
        get => _totalCount;
        private set { _totalCount = value; OnPropertyChanged(); }
    }

    public async Task LoadAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        ErrorMessage = null;
        Details = null;
        SelectedInvoice = null;
        try
        {
            var page = await invoiceApiClient.GetPageAsync(1, 50, accessToken, cancellationToken);
            Invoices.Clear();
            foreach (var invoice in page.Items)
                Invoices.Add(invoice);
            TotalCount = page.TotalCount;
            OnPropertyChanged(nameof(HasNoInvoices));
        }
        catch (InvoiceApiException exception)
        {
            ErrorMessage = exception.Message;
            Invoices.Clear();
            TotalCount = 0;
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "No se pudo conectar con el servidor. Verifica la conexión e inténtalo de nuevo.";
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            ErrorMessage = "El servidor tardó demasiado en responder. Inténtalo de nuevo.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadDetailsAsync(InvoiceListItemResponse invoice, string accessToken, CancellationToken cancellationToken = default)
    {
        SelectedInvoice = invoice;
        Details = null;
        DetailsErrorMessage = null;
        IsLoadingDetails = true;
        try
        {
            var details = await invoiceApiClient.GetDetailsAsync(invoice.Id, accessToken, cancellationToken);
            if (SelectedInvoice?.Id == invoice.Id)
                Details = details;
        }
        catch (InvoiceApiException exception)
        {
            DetailsErrorMessage = exception.Message;
        }
        catch (HttpRequestException)
        {
            DetailsErrorMessage = "No se pudo cargar el detalle. Verifica la conexión e inténtalo de nuevo.";
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            DetailsErrorMessage = "El servidor tardó demasiado en responder. Inténtalo de nuevo.";
        }
        finally
        {
            IsLoadingDetails = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

