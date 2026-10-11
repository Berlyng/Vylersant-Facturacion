using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Application.Products
{
    public class GetProductService
    {
        private readonly IProductRepository _productRepository;

        public GetProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IReadOnlyList<ProductListItem>> ExecuteAsync(Guid businessId, CancellationToken cancellationToken = default)
        {
            if (businessId == Guid.Empty)
            {
                throw new ArgumentException("El identificador del negocio es obligatorio.", nameof(businessId));
            }
            var products = await _productRepository.GetAllAsync(businessId, cancellationToken);
            return products.Select(p => new ProductListItem(p.Id, p.Name, p.Price, p.Sku, p.Description, p.IsActive)).ToList();
        }
    }
}
