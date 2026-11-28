namespace Vylersant_Facturacion.Application.Dashboard;

public interface IDashboardRepository
{
    Task<DashboardAggregate> GetAggregateAsync(Guid businessId, DateTime fromUtc, DateTime toUtc, string groupBy, CancellationToken cancellationToken = default);
    Task<DashboardPeriodTotals> GetTotalsAsync(Guid businessId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
}

