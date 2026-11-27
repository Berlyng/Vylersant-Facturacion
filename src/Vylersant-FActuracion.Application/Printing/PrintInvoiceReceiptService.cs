using Vylersant_Facturacion.Application.Customers;
using Vylersant_Facturacion.Application.Invoices;

namespace Vylersant_Facturacion.Application.Printing;

public sealed class PrintInvoiceReceiptService(
    IInvoiceRepository invoiceRepository,
    ICustomerRepository customerRepository,
    IReceiptPrinter receiptPrinter)
{
    public async Task<ReceiptPrintResult> ExecuteAsync(Guid invoiceId, Guid businessId, CancellationToken cancellationToken = default)
    {
        if (invoiceId == Guid.Empty)
            throw new ArgumentException("El identificador de la factura es obligatorio.", nameof(invoiceId));
        if (businessId == Guid.Empty)
            throw new ArgumentException("El identificador del negocio es obligatorio.", nameof(businessId));

        var invoice = await invoiceRepository.GetByIdAsync(invoiceId, businessId, cancellationToken)
            ?? throw new InvoiceNotFoundException(invoiceId);

        string? customerName = null;
        if (invoice.CustomerId is Guid customerId)
        {
            var customer = await customerRepository.GetByIdAsync(customerId, businessId, cancellationToken);
            customerName = customer?.Name;
        }

        var document = new ReceiptPrintDocument(
            invoice.Number,
            invoice.IssuedAtUtc,
            customerName,
            invoice.Total,
            invoice.Lines.Select(line => new ReceiptPrintLine(
                line.ProductName,
                line.Quantity,
                line.UnitPrice,
                line.LineTotal)).ToList());

        return await receiptPrinter.PrintAsync(document, cancellationToken);
    }
}

