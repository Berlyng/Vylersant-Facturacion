using System;
using System.Collections.Generic;
using System.Text;
using Vylersant_Facturacion.Application.Customers;
using Vylersant_Facturacion.Domain.Entities.Customers;
using Vylersant_Facturacion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Vylersant_Facturacion.Infrastructure.Repository
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly VylersantFacturacionDbContext _context;

        public CustomerRepository(VylersantFacturacionDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            await _context.Customers.AddAsync(customer, cancellationToken);
        }

        public async Task<IReadOnlyList<Customer>> GetAllAsync(Guid businessId, CancellationToken cancellationToken = default)
        {
            return await _context.Customers.Where(c => c.BusinessId == businessId).OrderBy(c => c.Name).ToListAsync(cancellationToken);
        }

        public async Task<Customer?> GetByIdAsync(Guid customerId, Guid businessId, CancellationToken cancellationToken = default)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerId && c.BusinessId == businessId, cancellationToken);
        }
    }
}
