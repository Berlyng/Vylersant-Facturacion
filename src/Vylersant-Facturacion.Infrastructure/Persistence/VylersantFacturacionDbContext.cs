using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Domain.Authentication;
using Vylersant_Facturacion.Domain.Entities.Businesses;
using Vylersant_Facturacion.Domain.Entities.Customers;
using Vylersant_Facturacion.Domain.Entities.Invoices;
using Vylersant_Facturacion.Domain.Entities.Products;
using Vylersant_Facturacion.Domain.Entities.Payments;
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
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<Payment> Payments => Set<Payment>();

        public async Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<CancellationToken, Task<TResult>> operation,
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);
            await using var transaction = await Database.BeginTransactionAsync(isolationLevel, cancellationToken);
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(VylersantFacturacionDbContext).Assembly);
        }
    }
}

