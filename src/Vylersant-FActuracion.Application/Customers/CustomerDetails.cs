namespace Vylersant_Facturacion.Application.Customers;

public sealed record CustomerDetails(
    Guid Id,
    string Name,
    string? Address,
    string? Phone,
    string? Email,
    bool IsActive);