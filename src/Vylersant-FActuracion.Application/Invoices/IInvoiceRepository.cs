using Vylersant_Facturacion.Domain.Entities.Invoices;

namespace Vylersant_Facturacion.Application.Invoices;

public interface IInvoiceRepository
{
    /// <summary>
    /// Devuelve la factura (con sus líneas) solo si pertenece al negocio indicado.
    /// </summary>
    Task<Invoice?> GetByIdAsync(
        Guid invoiceId,
        Guid businessId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve las facturas del negocio, de la más reciente a la más antigua.
    /// </summary>
    Task<IReadOnlyList<Invoice>> GetAllAsync(
        Guid businessId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Invoice>> GetPageAsync(
        Guid businessId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        Guid businessId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve el siguiente número de factura disponible para el negocio.
    /// </summary>
    Task<long> GetNextNumberAsync(
        Guid businessId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Invoice invoice,
        CancellationToken cancellationToken = default);
}

