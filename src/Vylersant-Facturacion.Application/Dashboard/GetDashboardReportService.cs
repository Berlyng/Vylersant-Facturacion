namespace Vylersant_Facturacion.Application.Dashboard;

public sealed class GetDashboardReportService(IDashboardRepository repository)
{
    public async Task<DashboardReport> ExecuteAsync(Guid businessId, DateTime fromUtc, DateTime toUtc, string groupBy = "day", CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty) throw new ArgumentException("El negocio es obligatorio.", nameof(businessId));
        if (fromUtc.Kind != DateTimeKind.Utc || toUtc.Kind != DateTimeKind.Utc || fromUtc >= toUtc)
            throw new ArgumentException("El rango debe usar fechas UTC y la fecha inicial debe ser anterior a la final.");
        if (groupBy is not ("day" or "month")) throw new ArgumentException("La agrupación debe ser 'day' o 'month'.", nameof(groupBy));

        var current = await repository.GetAggregateAsync(businessId, fromUtc, toUtc, groupBy, cancellationToken);
        var duration = toUtc - fromUtc;
        var previous = await repository.GetTotalsAsync(businessId, fromUtc - duration, fromUtc, cancellationToken);
        var change = previous.SalesTotal == 0
            ? current.Totals.SalesTotal == 0 ? 0m : 100m
            : decimal.Round((current.Totals.SalesTotal - previous.SalesTotal) / previous.SalesTotal * 100m, 2, MidpointRounding.AwayFromZero);
        var average = current.Totals.InvoiceCount == 0 ? 0m : decimal.Round(current.Totals.SalesTotal / current.Totals.InvoiceCount, 2, MidpointRounding.AwayFromZero);
        return new DashboardReport(fromUtc, toUtc, groupBy, current.Totals.SalesTotal, current.Totals.InvoiceCount, average,
            current.Totals.PaymentsCollected, previous.SalesTotal, previous.InvoiceCount, change, current.Trend);
    }
}

