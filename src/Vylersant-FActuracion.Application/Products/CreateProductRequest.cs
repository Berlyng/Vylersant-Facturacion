using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Application.Products
{
    public sealed record CreateProductRequest(string Name, decimal Price, string? Sku, string? Description);
   
}
