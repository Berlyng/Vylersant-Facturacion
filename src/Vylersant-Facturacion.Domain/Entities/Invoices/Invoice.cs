namespace Vylersant_Facturacion.Domain.Entities.Invoices;

public sealed class Invoice
{
    private readonly List<InvoiceLine> _lines = [];

    public Guid Id { get; private set; }

    public Guid BusinessId { get; private set; }

    public Guid? CustomerId { get; private set; }

    public long Number { get; private set; }

    public DateTime IssuedAtUtc { get; private set; }

    public InvoiceStatus Status { get; private set; }

    public decimal Total { get; private set; }

    public DateTime? CancelledAtUtc { get; private set; }

    public IReadOnlyCollection<InvoiceLine> Lines =>
        _lines.AsReadOnly();

    private Invoice()
    {
    }

    public Invoice(
        Guid businessId,
        Guid? customerId,
        long number,
        DateTime issuedAtUtc,
        IEnumerable<InvoiceLine> lines)
    {
        if (businessId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del negocio es obligatorio.",
                nameof(businessId));
        }

        if (customerId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del cliente no puede estar vacío.",
                nameof(customerId));
        }

        if (number <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(number),
                "El número de factura debe ser mayor que cero.");
        }

        ArgumentNullException.ThrowIfNull(lines);

        var invoiceLines = lines.ToList();

        if (invoiceLines.Count == 0)
        {
            throw new ArgumentException(
                "La factura debe contener al menos una línea.",
                nameof(lines));
        }

        Id = Guid.NewGuid();
        BusinessId = businessId;
        CustomerId = customerId;
        Number = number;
        IssuedAtUtc = issuedAtUtc;
        Status = InvoiceStatus.Issued;

        _lines.AddRange(invoiceLines);

        Total = _lines.Sum(
            line => line.LineTotal);
    }

    public void Cancel(DateTime cancelledAtUtc)
    {
        if (Status == InvoiceStatus.Cancelled)
        {
            return;
        }

        Status = InvoiceStatus.Cancelled;
        CancelledAtUtc = cancelledAtUtc;
    }
}