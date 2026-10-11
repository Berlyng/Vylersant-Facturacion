using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Domain.Entities.Products;

namespace Vylersant_Facturacion.Application.Products
{
    public class CreateProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateProductService(IProductRepository productRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<CreateProductResult> ExecuteAsync(Guid businessId, CreateProductRequest request, CancellationToken cancellationToken = default)
        {
            var product = new Product(businessId, request.Name, request.Price, request.Sku, request.Description);
            await _productRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new CreateProductResult(product.Id);
        }
    }
}
