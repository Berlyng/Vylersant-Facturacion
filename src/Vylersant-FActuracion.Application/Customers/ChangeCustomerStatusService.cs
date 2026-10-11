using Vylersant_Facturacion.Application.Abstraccion;

namespace Vylersant_Facturacion.Application.Customers;

public sealed class ChangeCustomerStatusService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeCustomerStatusService(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(
        Guid customerId,
        Guid businessId,
        bool isActive,
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

        if (isActive)
        {
            customer.Activate();
        }
        else
        {
            customer.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}