using Vylersant_Facturacion.Application.Customers;

namespace Vylersant_Facturacion.Application.Invoices;

public sealed class GetInvoiceByIdService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICustomerRepository _customerRepository;

    public GetInvoiceByIdService(
        IInvoiceRepository invoiceRepository,
        ICustomerRepository customerRepository)
    {
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
    }

    public async Task<InvoiceDetails> ExecuteAsync(
        Guid invoiceId,
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        if (invoiceId == Guid.Empty)
        {
            throw new ArgumentException("El identificador de la factura es obligatorio.", nameof(invoiceId));
        }

        if (businessId == Guid.Empty)
        {
            throw new ArgumentException("El identificador del negocio es obligatorio.", nameof(businessId));
        }

        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, businessId, cancellationToken)
            ?? throw new InvoiceNotFoundException(invoiceId);

        string? customerName = null;
        if (invoice.CustomerId is Guid customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId, businessId, cancellationToken);
            customerName = customer?.Name;
        }

        return new InvoiceDetails(
            invoice.Id,
            invoice.Number,
            invoice.IssuedAtUtc,
            invoice.Status,
            invoice.CustomerId,
            customerName,
            invoice.Total,
            invoice.CancelledAtUtc,
            invoice.Lines.Select(line => new InvoiceLineDetails(
                line.ProductId,
                line.ProductName,
                line.Quantity,
                line.UnitPrice,
                line.LineTotal)).ToList());
    }
}

