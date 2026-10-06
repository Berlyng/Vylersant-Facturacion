using System;
using System.Collections.Generic;
using System.Text;
using Vylersant_Facturacion.Domain.Entities.Users;

namespace Vylersant_Facturacion.Application.Users
{
    public interface IUserRepository
    {
        Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken);
        Task AddAsync(User user, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
