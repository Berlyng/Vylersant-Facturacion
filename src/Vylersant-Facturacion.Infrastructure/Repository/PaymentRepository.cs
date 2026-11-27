using Microsoft.EntityFrameworkCore;
using Vylersant_Facturacion.Application.Payments;
using Vylersant_Facturacion.Domain.Entities.Payments;
using Vylersant_Facturacion.Infrastructure.Persistence;

namespace Vylersant_Facturacion.Infrastructure.Repository;

public sealed class PaymentRepository(VylersantFacturacionDbContext context) : IPaymentRepository
{
    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default) =>
        await context.Payments.AddAsync(payment, cancellationToken);

    public async Task<decimal> GetCompletedTotalAsync(Guid invoiceId, Guid businessId, CancellationToken cancellationToken = default) =>
        await context.Payments.Where(p => p.InvoiceId == invoiceId && p.BusinessId == businessId && p.Status == PaymentStatus.Completed)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;
}

