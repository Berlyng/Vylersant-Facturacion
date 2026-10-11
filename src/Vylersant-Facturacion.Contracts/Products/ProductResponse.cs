namespace Vylersant_Facturacion.Contracts.Products;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    decimal Price,
    string? Sku,
    string? Description,
    bool IsActive);