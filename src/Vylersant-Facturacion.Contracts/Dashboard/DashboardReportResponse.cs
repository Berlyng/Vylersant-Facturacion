namespace Vylersant_Facturacion.Contracts.Dashboard;

public sealed record DashboardReportResponse(
    DateTime FromUtc, DateTime ToUtc, string GroupBy, decimal SalesTotal, long InvoiceCount, decimal AverageTicket,
    decimal PaymentsCollected, decimal PreviousSalesTotal, long PreviousInvoiceCount, decimal SalesChangePercent,
    IReadOnlyList<DashboardTrendPointResponse> Trend);

public sealed record DashboardTrendPointResponse(DateTime PeriodStartUtc, decimal SalesTotal, long InvoiceCount, decimal PaymentsCollected);

