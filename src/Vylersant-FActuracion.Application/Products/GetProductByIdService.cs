using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Application.Products
{
    public class GetProductByIdService
    {
        private readonly IProductRepository _productRepository;

        public GetProductByIdService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductDetails> ExecuteAsync(Guid productId, Guid businessId, CancellationToken cancellationToken)
        {
            if (productId == Guid.Empty)
            {
                throw new ArgumentException("El identificador del producto no puede estar vacío.", nameof(productId));
            }
            if (businessId == Guid.Empty)
            {
                throw new ArgumentException("El identificador de la empresa no puede estar vacío.", nameof(businessId));
            }

            
            var product = await _productRepository.GetByIdAsync(productId, businessId, cancellationToken);
            if (product == null)
            {
                throw new ProductNotFoundException();
            }
            return new ProductDetails(
                product.Id,
                product.Name,
                product.Price,
                product.Sku,
                product.Description,
                product.IsActive
            );
        }
    }
}
