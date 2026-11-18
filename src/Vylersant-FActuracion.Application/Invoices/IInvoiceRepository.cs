using Vylersant_Facturacion.Domain.Entities.Invoices;

namespace Vylersant_Facturacion.Application.Invoices;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid invoiceId, Guid businessId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Invoice>> GetAllAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<long> GetNextNumberAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
}
