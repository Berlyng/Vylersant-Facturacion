using System;
using System.Collections.Generic;
using System.Text;
using Vylersant_Facturacion.Application.Abstraccion;

namespace Vylersant_Facturacion.Application.Products
{
    public class UpdateProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProductService(IProductRepository productRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync(Guid productId, Guid businessId, UpdateProductRequest request, CancellationToken cancellationToken = default)
        {
            if(productId == Guid.Empty)
            {
                throw new ArgumentException("El ID del producto no puede estar vacío.", nameof(productId));

            }
            if (businessId == Guid.Empty)
            {
                throw new ArgumentException("El ID del negocio no puede estar vacío.", nameof(businessId));
            }
            var product = await _productRepository.GetByIdAsync(productId, businessId, cancellationToken);
            if (product == null)
            {
                throw new ProductNotFoundException();
            }

            product.ChangeName(request.Name);
            product.ChangePrice(request.Price);
            product.ChangeSku(request.Sku);
            product.ChangeDescription(request.Description);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
