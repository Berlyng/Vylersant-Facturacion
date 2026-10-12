namespace Vylersant_Facturacion.Domain.Entities.Invoices;

public sealed class InvoiceLine
{
    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; }

    public decimal Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal LineTotal { get; private set; }

    private InvoiceLine()
    {
        ProductName = string.Empty;
    }

    public InvoiceLine(
        Guid productId,
        string productName,
        decimal quantity,
        decimal unitPrice)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del producto es obligatorio.",
                nameof(productId));
        }

        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new ArgumentException(
                "El nombre del producto es obligatorio.",
                nameof(productName));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "La cantidad debe ser mayor que cero.");
        }

        if (unitPrice < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(unitPrice),
                "El precio unitario no puede ser negativo.");
        }

        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = quantity * unitPrice;
    }
}