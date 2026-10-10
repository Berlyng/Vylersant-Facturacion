namespace Vylersant_Facturacion.Domain.Entities.Products;

public sealed class Product
{
    public Guid Id { get; private set; }

    public Guid BusinessId { get; private set; }

    public string Name { get; private set; }

    public decimal Price { get; private set; }

    public string? Sku { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    private Product()
    {
        Name = string.Empty;
    }

    public Product(
        Guid businessId,
        string name,
        decimal price,
        string? sku = null,
        string? description = null)
    {
        if (businessId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del negocio es obligatorio.",
                nameof(businessId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "El nombre del producto es obligatorio.",
                nameof(name));
        }

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(price),
                "El precio no puede ser negativo.");
        }

        Id = Guid.NewGuid();
        BusinessId = businessId;
        Name = name.Trim();
        Price = price;
        Sku = NormalizeOptionalText(sku);
        Description = NormalizeOptionalText(description);
        IsActive = true;
    }

    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "El nombre del producto es obligatorio.",
                nameof(name));
        }

        Name = name.Trim();
    }

    public void ChangePrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(price),
                "El precio no puede ser negativo.");
        }

        Price = price;
    }

    public void ChangeSku(string? sku)
    {
        Sku = NormalizeOptionalText(sku);
    }

    public void ChangeDescription(string? description)
    {
        Description = NormalizeOptionalText(description);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}