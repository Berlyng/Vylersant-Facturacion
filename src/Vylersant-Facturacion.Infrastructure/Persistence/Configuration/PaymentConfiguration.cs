using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vylersant_Facturacion.Domain.Entities.Payments;

namespace Vylersant_Facturacion.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.BusinessId).IsRequired();
        builder.Property(x => x.InvoiceId).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.PaidAtUtc).IsRequired();
        builder.Property(x => x.Method).IsRequired();
        builder.Property(x => x.Reference).HasMaxLength(200);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.VoidedAtUtc);
        builder.HasIndex(x => new { x.BusinessId, x.InvoiceId, x.Status });
        builder.HasOne<Vylersant_Facturacion.Domain.Entities.Invoices.Invoice>()
            .WithMany().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Vylersant_Facturacion.Domain.Entities.Businesses.Business>()
            .WithMany().HasForeignKey(x => x.BusinessId).OnDelete(DeleteBehavior.Restrict);
    }
}

