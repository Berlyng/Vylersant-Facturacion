using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Domain.Authentication;
using Vylersant_Facturacion.Domain.Entities.Businesses;
using Vylersant_Facturacion.Domain.Entities.Users;

namespace Vylersant_Facturacion.Infrastructure.Persistence
{
    public sealed class VylersantFacturacionDbContext : DbContext, IUnitOfWork
    {
        public VylersantFacturacionDbContext(DbContextOptions<VylersantFacturacionDbContext> options)
            : base(options)
        {

        }

        public DbSet<Business> Businesses => Set<Business>();

        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(VylersantFacturacionDbContext).Assembly);
        }
    }
}
