using Vylersant_Facturacion.Domain.Entities.Customers;

namespace Vylersant_Facturacion.Application.Customers;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(
        Guid customerId,
        Guid businessId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Customer>> GetAllAsync(
        Guid businessId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Customer customer,
        CancellationToken cancellationToken = default);
}