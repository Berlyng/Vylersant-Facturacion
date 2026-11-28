using Vylersant_Facturacion.Contracts.Dashboard;

namespace Vylersant_Facturacion.Desktop.Dashboard;

public interface IDashboardApiClient
{
    Task<DashboardReportResponse> GetReportAsync(DateTime fromUtc, DateTime toUtc, string groupBy, string accessToken, CancellationToken cancellationToken = default);
}

