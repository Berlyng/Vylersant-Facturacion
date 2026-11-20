using Microsoft.EntityFrameworkCore;
using Vylersant_Facturacion.Application.Invoices;
using Vylersant_Facturacion.Domain.Entities.Invoices;
using Vylersant_Facturacion.Infrastructure.Persistence;

namespace Vylersant_Facturacion.Infrastructure.Repository;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly VylersantFacturacionDbContext _context;

    public InvoiceRepository(VylersantFacturacionDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Invoice invoice,
        CancellationToken cancellationToken = default)
    {
        await _context.Invoices.AddAsync(
            invoice,
            cancellationToken);
    }

    public async Task<Invoice?> GetByIdAsync(
        Guid invoiceId,
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        // Con seguimiento (tracking): CancelInvoiceService modifica la entidad devuelta.
        return await _context.Invoices
            .Include(invoice => invoice.Lines)
            .FirstOrDefaultAsync(
                invoice =>
                    invoice.Id == invoiceId &&
                    invoice.BusinessId == businessId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> GetAllAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .AsNoTracking()
            .Where(invoice => invoice.BusinessId == businessId)
            .OrderByDescending(invoice => invoice.Number)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> GetPageAsync(
        Guid businessId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .AsNoTracking()
            .Where(invoice => invoice.BusinessId == businessId)
            .OrderByDescending(invoice => invoice.IssuedAtUtc)
            .ThenByDescending(invoice => invoice.Number)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .LongCountAsync(invoice => invoice.BusinessId == businessId, cancellationToken);
    }

    public async Task<long> GetNextNumberAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var lastNumber =
            await _context.Invoices
                .Where(invoice => invoice.BusinessId == businessId)
                .MaxAsync(
                    invoice => (long?)invoice.Number,
                    cancellationToken);

        return (lastNumber ?? 0) + 1;
    }
}

