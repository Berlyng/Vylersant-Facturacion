using System;
using System.Collections.Generic;
using System.Text;
using Vylersant_Facturacion.Domain.Entities.Businesses;

namespace Vylersant_Facturacion.Application.Bussinesses
{
    public interface IBussinesRepository
    {
        Task AddAsync(Business bussines, CancellationToken cancellationToken = default);
    }
}
