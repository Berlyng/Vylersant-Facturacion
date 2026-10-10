using Vylersant_Facturacion.Application.Users;
using Vylersant_Facturacion.Domain.Entities.Users;
using Vylersant_Facturacion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Vylersant_Facturacion.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {

        private readonly VylersantFacturacionDbContext _dbContext;

        public UserRepository(VylersantFacturacionDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken)
        {
            await _dbContext.Users.AddAsync(user, cancellationToken);
        }

        public async Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        }

        public async Task<User> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }
    }
}

        