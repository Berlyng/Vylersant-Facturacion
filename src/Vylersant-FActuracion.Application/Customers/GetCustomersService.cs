namespace Vylersant_Facturacion.Application.Customers;

public sealed class GetCustomersService
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomersService(
        ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<IReadOnlyList<CustomerListItem>> ExecuteAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del negocio es obligatorio.",
                nameof(businessId));
        }

        var customers =
            await _customerRepository.GetAllAsync(
                businessId,
                cancellationToken);

        return customers
            .Select(customer =>
                new CustomerListItem(
                    customer.Id,
                    customer.Name,
                    customer.Address,
                    customer.Phone,
                    customer.Email,
                    customer.IsActive))
            .ToList();
    }
}