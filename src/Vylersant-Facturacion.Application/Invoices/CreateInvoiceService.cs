using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Customers;
using Vylersant_Facturacion.Application.Products;
using Vylersant_Facturacion.Domain.Entities.Invoices;
using System.Data;

namespace Vylersant_Facturacion.Application.Invoices;

public sealed class CreateInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public CreateInvoiceService(
        IInvoiceRepository invoiceRepository,
        IProductRepository productRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _invoiceRepository = invoiceRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<CreateInvoiceResult> ExecuteAsync(
        Guid businessId,
        Guid issuedByUserId,
        CreateInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (businessId == Guid.Empty)
        {
            throw new ArgumentException("El identificador del negocio es obligatorio.", nameof(businessId));
        }

        if (issuedByUserId == Guid.Empty)
        {
            throw new ArgumentException("El identificador del usuario emisor es obligatorio.", nameof(issuedByUserId));
        }

        if (request.CustomerId == Guid.Empty)
        {
            throw new ArgumentException("El identificador del cliente no puede estar vacío.", nameof(request));
        }

        if (request.Lines is null || request.Lines.Count == 0)
        {
            throw new ArgumentException("La factura debe contener al menos una línea.", nameof(request));
        }

        if (request.CustomerId is Guid customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId, businessId, cancellationToken);
            if (customer is null || !customer.IsActive)
            {
                throw new CustomerNotFoundException();
            }
        }

        var invoiceLines = new List<InvoiceLine>(request.Lines.Count);
        foreach (var requestedLine in request.Lines)
        {
            if (requestedLine is null)
            {
                throw new ArgumentException("Las líneas de factura no pueden ser nulas.", nameof(request));
            }

            if (requestedLine.ProductId == Guid.Empty)
            {
                throw new ArgumentException("El identificador del producto es obligatorio.", nameof(request));
            }

            if (requestedLine.Quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(request), "La cantidad debe ser mayor que cero.");
            }

            var product = await _productRepository.GetByIdAsync(
                requestedLine.ProductId,
                businessId,
                cancellationToken);

            if (product is null || !product.IsActive)
            {
                throw new ProductNotFoundException();
            }

            invoiceLines.Add(new InvoiceLine(
                product.Id,
                product.Name,
                requestedLine.Quantity,
                product.Price));
        }

        return await _unitOfWork.ExecuteInTransactionAsync(
            async transactionToken =>
            {
                var number = await _invoiceRepository.GetNextNumberAsync(businessId, transactionToken);
                var invoice = new Invoice(
                    businessId,
                    issuedByUserId,
                    request.CustomerId,
                    number,
                    _timeProvider.GetUtcNow().UtcDateTime,
                    invoiceLines);

                await _invoiceRepository.AddAsync(invoice, transactionToken);
                await _unitOfWork.SaveChangesAsync(transactionToken);

                return new CreateInvoiceResult(invoice.Id, invoice.Number, invoice.Total);
            },
            IsolationLevel.Serializable,
            cancellationToken);
    }
}

