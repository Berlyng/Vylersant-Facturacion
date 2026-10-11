namespace Vylersant_Facturacion.Domain.Entities.Customers;

public sealed class Customer
{
    public Guid Id { get; private set; }

    public Guid BusinessId { get; private set; }

    public string Name { get; private set; }

    public string? Address { get; private set; }

    public string? Phone { get; private set; }

    public string? Email { get; private set; }

    public bool IsActive { get; private set; }

    private Customer()
    {
        Name = string.Empty;
    }

    public Customer(
        Guid businessId,
        string name,
        string? address = null,
        string? phone = null,
        string? email = null)
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
                "El nombre del cliente es obligatorio.",
                nameof(name));
        }

        Id = Guid.NewGuid();
        BusinessId = businessId;
        Name = name.Trim();
        Address = NormalizeOptionalText(address);
        Phone = NormalizeOptionalText(phone);
        Email = NormalizeEmail(email);
        IsActive = true;
    }

    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "El nombre del cliente es obligatorio.",
                nameof(name));
        }

        Name = name.Trim();
    }

    public void ChangeAddress(string? address)
    {
        Address = NormalizeOptionalText(address);
    }

    public void ChangePhone(string? phone)
    {
        Phone = NormalizeOptionalText(phone);
    }

    public void ChangeEmail(string? email)
    {
        Email = NormalizeEmail(email);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string? NormalizeEmail(
        string? email)
    {
        return string.IsNullOrWhiteSpace(email)
            ? null
            : email.Trim().ToLowerInvariant();
    }
}