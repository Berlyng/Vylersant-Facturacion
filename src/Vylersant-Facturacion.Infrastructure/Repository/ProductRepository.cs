using Vylersant_Facturacion.Application.Products;
using Vylersant_Facturacion.Domain.Entities.Products;
using Vylersant_Facturacion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Vylersant_Facturacion.Infrastructure.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly VylersantFacturacionDbContext _dbContext;

        public ProductRepository(VylersantFacturacionDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            await _dbContext.Products.AddAsync(product, cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> GetAllAsync(Guid businessId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Products.Where(p => p.BusinessId == businessId).OrderBy(p => p.Name).ToListAsync(cancellationToken);
        }

        public async Task<Product?> GetByIdAsync(Guid productId, Guid businessId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
