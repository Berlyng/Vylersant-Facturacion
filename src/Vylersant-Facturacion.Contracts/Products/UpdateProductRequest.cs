namespace Vylersant_Facturacion.Contracts.Products;

public sealed record UpdateProductRequest(
    string Name,
    decimal Price,
    string? Sku,
    string? Description);