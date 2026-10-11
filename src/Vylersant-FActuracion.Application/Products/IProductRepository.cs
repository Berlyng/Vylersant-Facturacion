using System;
using System.Collections.Generic;
using System.Text;
using Vylersant_Facturacion.Domain.Entities.Products;

namespace Vylersant_Facturacion.Application.Products
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid productId, Guid businessId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Product>> GetAllAsync(Guid businessId, CancellationToken cancellationToken = default);
        Task AddAsync(Product product, CancellationToken cancellationToken = default);
    }
}
