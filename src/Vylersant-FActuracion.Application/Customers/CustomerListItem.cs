namespace Vylersant_Facturacion.Application.Customers;

public sealed record CustomerListItem(
    Guid Id,
    string Name,
    string? Address,
    string? Phone,
    string? Email,
    bool IsActive);