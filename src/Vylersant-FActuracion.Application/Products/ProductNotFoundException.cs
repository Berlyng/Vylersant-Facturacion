namespace Vylersant_Facturacion.Application.Products;

public sealed class ProductNotFoundException : Exception
{
    public ProductNotFoundException()
        : base("El producto no fue encontrado.")
    {
    }
}