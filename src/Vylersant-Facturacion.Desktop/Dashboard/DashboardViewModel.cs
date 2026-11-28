using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Vylersant_Facturacion.Contracts.Dashboard;

namespace Vylersant_Facturacion.Desktop.Dashboard;

public sealed class DashboardViewModel(IDashboardApiClient apiClient) : INotifyPropertyChanged
{
    private DateTime _fromDate = DateTime.UtcNow.Date.AddDays(-29);
    private DateTime _toDate = DateTime.UtcNow.Date;
    private string _groupBy = "day";
    private bool _isLoading;
    private string? _errorMessage;
    private DashboardReportResponse? _report;

    public DateTime FromDate { get => _fromDate; set { _fromDate = value.Date; OnPropertyChanged(); } }
    public DateTime ToDate { get => _toDate; set { _toDate = value.Date; OnPropertyChanged(); } }
    public string GroupBy { get => _groupBy; set { _groupBy = value; OnPropertyChanged(); } }
    public bool IsLoading { get => _isLoading; private set { _isLoading = value; OnPropertyChanged(); } }
    public string? ErrorMessage { get => _errorMessage; private set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); } }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public DashboardReportResponse? Report { get => _report; private set { _report = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasReport)); OnPropertyChanged(nameof(MaxTrendSales)); } }
    public bool HasReport => Report is not null;
    public decimal MaxTrendSales
    {
        get
        {
            var max = Report?.Trend.Select(point => point.SalesTotal).DefaultIfEmpty(0m).Max() ?? 0m;
            return max > 0m ? max : 1m;
        }
    }
    public ObservableCollection<DashboardTrendPointResponse> Trend { get; } = [];

    public async Task LoadAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        if (FromDate > ToDate)
        {
            ErrorMessage = "La fecha inicial debe ser anterior o igual a la final.";
            return;
        }
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var from = DateTime.SpecifyKind(FromDate.Date, DateTimeKind.Utc);
            var toExclusive = DateTime.SpecifyKind(ToDate.Date.AddDays(1), DateTimeKind.Utc);
            Report = await apiClient.GetReportAsync(from, toExclusive, GroupBy, accessToken, cancellationToken);
            Trend.Clear();
            foreach (var point in Report.Trend)
                Trend.Add(point);
            OnPropertyChanged(nameof(MaxTrendSales));
        }
        catch (DashboardApiException exception) { ErrorMessage = exception.Message; }
        catch (HttpRequestException) { ErrorMessage = "No se pudo conectar con el servidor."; }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested) { ErrorMessage = "El servidor tardó demasiado en responder."; }
        finally { IsLoading = false; }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

