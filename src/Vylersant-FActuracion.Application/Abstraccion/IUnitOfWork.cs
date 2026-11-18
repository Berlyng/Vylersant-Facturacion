using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Vylersant_Facturacion.Application.Abstraccion
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<CancellationToken, Task<TResult>> operation,
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default)
        {
            return operation(cancellationToken);
        }
    }
}

