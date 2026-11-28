using Microsoft.EntityFrameworkCore;
using Vylersant_Facturacion.Application.Dashboard;
using Vylersant_Facturacion.Domain.Entities.Invoices;
using Vylersant_Facturacion.Domain.Entities.Payments;
using Vylersant_Facturacion.Infrastructure.Persistence;

namespace Vylersant_Facturacion.Infrastructure.Repository;

public sealed class DashboardRepository(VylersantFacturacionDbContext context) : IDashboardRepository
{
    public async Task<DashboardAggregate> GetAggregateAsync(Guid businessId, DateTime fromUtc, DateTime toUtc, string groupBy, CancellationToken cancellationToken = default)
    {
        var invoiceGroups = await context.Invoices.AsNoTracking()
            .Where(i => i.BusinessId == businessId && i.Status == InvoiceStatus.Issued && i.IssuedAtUtc >= fromUtc && i.IssuedAtUtc < toUtc)
            .GroupBy(i => new { Year = i.IssuedAtUtc.Year, Month = i.IssuedAtUtc.Month, Day = groupBy == "day" ? i.IssuedAtUtc.Day : 1 })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Day, Total = g.Sum(i => i.Total), Count = g.LongCount() })
            .ToListAsync(cancellationToken);

        var paymentGroups = await context.Payments.AsNoTracking()
            .Where(p => p.BusinessId == businessId && p.Status == PaymentStatus.Completed &&
                        context.Invoices.Any(i => i.Id == p.InvoiceId && i.BusinessId == businessId && i.Status == InvoiceStatus.Issued) &&
                        p.PaidAtUtc >= fromUtc && p.PaidAtUtc < toUtc)
            .GroupBy(p => new { Year = p.PaidAtUtc.Year, Month = p.PaidAtUtc.Month, Day = groupBy == "day" ? p.PaidAtUtc.Day : 1 })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Day, Total = g.Sum(p => p.Amount) })
            .ToListAsync(cancellationToken);

        var sales = invoiceGroups.ToDictionary(g => new DateTime(g.Year, g.Month, g.Day, 0, 0, 0, DateTimeKind.Utc), g => (g.Total, g.Count));
        var collections = paymentGroups.ToDictionary(g => new DateTime(g.Year, g.Month, g.Day, 0, 0, 0, DateTimeKind.Utc), g => g.Total);
        var buckets = new List<DashboardTrendPoint>();
        var cursor = groupBy == "day"
            ? new DateTime(fromUtc.Year, fromUtc.Month, fromUtc.Day, 0, 0, 0, DateTimeKind.Utc)
            : new DateTime(fromUtc.Year, fromUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        while (cursor < toUtc)
        {
            sales.TryGetValue(cursor, out var daySales);
            collections.TryGetValue(cursor, out var paid);
            buckets.Add(new DashboardTrendPoint(cursor, daySales.Total, daySales.Count, paid));
            cursor = groupBy == "day" ? cursor.AddDays(1) : cursor.AddMonths(1);
        }

        return new DashboardAggregate(
            new DashboardPeriodTotals(invoiceGroups.Sum(g => g.Total), invoiceGroups.Sum(g => g.Count), paymentGroups.Sum(g => g.Total)),
            buckets);
    }

    public async Task<DashboardPeriodTotals> GetTotalsAsync(Guid businessId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var invoices = context.Invoices.AsNoTracking().Where(i => i.BusinessId == businessId && i.Status == InvoiceStatus.Issued && i.IssuedAtUtc >= fromUtc && i.IssuedAtUtc < toUtc);
        var payments = context.Payments.AsNoTracking().Where(p => p.BusinessId == businessId && p.Status == PaymentStatus.Completed &&
            context.Invoices.Any(i => i.Id == p.InvoiceId && i.BusinessId == businessId && i.Status == InvoiceStatus.Issued) &&
            p.PaidAtUtc >= fromUtc && p.PaidAtUtc < toUtc);
        var sales = await invoices.SumAsync(i => (decimal?)i.Total, cancellationToken) ?? 0m;
        var count = await invoices.LongCountAsync(cancellationToken);
        var paid = await payments.SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;
        return new DashboardPeriodTotals(sales, count, paid);
    }
}

