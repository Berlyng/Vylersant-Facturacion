namespace Vylersant_Facturacion.Contracts.Customers;

public sealed record CustomerResponse(
    Guid Id,
    string Name,
    string? Address,
    string? Phone,
    string? Email,
    bool IsActive);