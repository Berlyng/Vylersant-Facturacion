using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vylersant_Facturacion.Domain.Entities.Businesses;
using Vylersant_Facturacion.Domain.Entities.Customers;
using Vylersant_Facturacion.Domain.Entities.Invoices;

namespace Vylersant_Facturacion.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.BusinessId)
            .IsRequired();

        builder.Property(x => x.CustomerId)
            .IsRequired(false);

        builder.Property(x => x.Number)
            .HasMaxLength(18)
            .IsRequired();

        builder.Property(x => x.IssuedAtUtc)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.Total)
            .HasPrecision(18,2)
            .IsRequired();

        builder.Property(x => x.CancelledAtUtc)
            .IsRequired(false);

        // REQUISITO: Índice único compuesto para impedir números de factura duplicados dentro de un mismo negocio
        builder.HasIndex(x => new { x.BusinessId, x.Number })
            .IsUnique();

        // REQUISITO: Relación con Business (Protegiendo el historial con Restrict)
        builder.HasOne<Business>() 
            .WithMany()
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        // REQUISITO: Relación con Customer (Opcional, con Restrict o SetNull)
        builder.HasOne<Customer>() 
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación con las líneas de detalle (1 a N)
        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey("InvoiceId")
            .OnDelete(DeleteBehavior.Restrict); // Las líneas sí se eliminan si se elimina la factura
    }
}