namespace Vylersant_Facturacion.Domain.Entities.Payments;

public sealed class Payment
{
    public Guid Id { get; private set; }

    public Guid BusinessId { get; private set; }

    public Guid InvoiceId { get; private set; }

    public decimal Amount { get; private set; }

    public DateTime PaidAtUtc { get; private set; }

    public PaymentMethod Method { get; private set; }

    public string? Reference { get; private set; }

    public PaymentStatus Status { get; private set; }

    public DateTime? VoidedAtUtc { get; private set; }

    public string? VoidReason { get; private set; }

    private Payment()
    {
    }

    public Payment(
        Guid businessId,
        Guid invoiceId,
        decimal amount,
        DateTime paidAtUtc,
        PaymentMethod method,
        string? reference = null)
    {
        if (businessId == Guid.Empty)
        {
            throw new ArgumentException("El identificador del negocio es obligatorio.", nameof(businessId));
        }

        if (invoiceId == Guid.Empty)
        {
            throw new ArgumentException("El identificador de la factura es obligatorio.", nameof(invoiceId));
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "El importe debe ser mayor que cero.");
        }

        var roundedAmount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        if (roundedAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "El importe debe ser al menos un centavo.");
        }

        if (!Enum.IsDefined(method))
        {
            throw new ArgumentOutOfRangeException(nameof(method), "El método de pago no es válido.");
        }

        Id = Guid.NewGuid();
        BusinessId = businessId;
        InvoiceId = invoiceId;
        Amount = roundedAmount;
        PaidAtUtc = paidAtUtc;
        Method = method;
        Reference = NormalizeOptionalText(reference);
        Status = PaymentStatus.Completed;
    }

    public void Void(DateTime voidedAtUtc, string reason)
    {
        if (Status == PaymentStatus.Voided)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("El motivo de anulación es obligatorio.", nameof(reason));
        }

        Status = PaymentStatus.Voided;
        VoidedAtUtc = voidedAtUtc;
        VoidReason = reason.Trim();
    }

    private static string? NormalizeOptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

