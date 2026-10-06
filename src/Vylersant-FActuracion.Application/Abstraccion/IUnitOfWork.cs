using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Application.Abstraccion
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
