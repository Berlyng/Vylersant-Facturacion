using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Domain.Entities.Customers;

namespace Vylersant_Facturacion.Application.Customers;

public sealed class CreateCustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerService(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateCustomerResult> ExecuteAsync(
        Guid businessId,
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = new Customer(
            businessId,
            request.Name,
            request.Address,
            request.Phone,
            request.Email);

        await _customerRepository.AddAsync(
            customer,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new CreateCustomerResult(
            customer.Id);
    }
}