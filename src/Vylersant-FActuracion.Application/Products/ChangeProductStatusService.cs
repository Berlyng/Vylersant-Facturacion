using System;
using System.Collections.Generic;
using System.Text;
using Vylersant_Facturacion.Application.Abstraccion;

namespace Vylersant_Facturacion.Application.Products
{
    public class ChangeProductStatusService
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ChangeProductStatusService(IProductRepository productRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync(Guid productId, Guid businessId, bool isActive, CancellationToken cancellationToken = default)
        {
            if(productId == null)
            {
                throw new ArgumentException("El ID del producto es obligatorio");
            }
            if (businessId == null)
            {
                throw new ArgumentException("El ID del negocio es obligatorio");
            }

            var product = await _productRepository.GetByIdAsync(productId, businessId, cancellationToken);
            if(product is null)
            {
                throw new ProductNotFoundException();
            }
            if (isActive)
            {
                product.Activate();
            }
            else
            {
                product.Deactivate();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
