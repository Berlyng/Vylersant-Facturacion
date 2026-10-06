using System;
using System.Collections.Generic;
using System.Text;
using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Bussinesses;
using Vylersant_Facturacion.Domain.Entities.Businesses;
using Vylersant_Facturacion.Infrastructure.Persistence;

namespace Vylersant_Facturacion.Infrastructure.Repository
{
    public class BussinesRepository : IBussinesRepository
    {
        private readonly VylersantFacturacionDbContext _dbContext;
        public async Task AddAsync(Business bussines, CancellationToken cancellationToken = default)
        {
             await _dbContext.Businesses.AddAsync(bussines, cancellationToken);
        }
    }
}
