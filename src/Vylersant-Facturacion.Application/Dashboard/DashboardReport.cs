namespace Vylersant_Facturacion.Application.Dashboard;

public sealed record DashboardReport(
    DateTime FromUtc,
    DateTime ToUtc,
    string GroupBy,
    decimal SalesTotal,
    long InvoiceCount,
    decimal AverageTicket,
    decimal PaymentsCollected,
    decimal PreviousSalesTotal,
    long PreviousInvoiceCount,
    decimal SalesChangePercent,
    IReadOnlyList<DashboardTrendPoint> Trend);

public sealed record DashboardTrendPoint(DateTime PeriodStartUtc, decimal SalesTotal, long InvoiceCount, decimal PaymentsCollected);
public sealed record DashboardPeriodTotals(decimal SalesTotal, long InvoiceCount, decimal PaymentsCollected);
public sealed record DashboardAggregate(DashboardPeriodTotals Totals, IReadOnlyList<DashboardTrendPoint> Trend);

