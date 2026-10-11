namespace Vylersant_Facturacion.Application.Customers;

public sealed class GetCustomerByIdService
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByIdService(
        ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerDetails> ExecuteAsync(
        Guid customerId,
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del cliente es obligatorio.",
                nameof(customerId));
        }

        if (businessId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del negocio es obligatorio.",
                nameof(businessId));
        }

        var customer =
            await _customerRepository.GetByIdAsync(
                customerId,
                businessId,
                cancellationToken);

        if (customer is null)
        {
            throw new CustomerNotFoundException();
        }

        return new CustomerDetails(
            customer.Id,
            customer.Name,
            customer.Address,
            customer.Phone,
            customer.Email,
            customer.IsActive);
    }
}