namespace Vylersant_Facturacion.Contracts.Products;

public sealed record CreateProductRequest(
    string Name,
    decimal Price,
    string? Sku,
    string? Description);