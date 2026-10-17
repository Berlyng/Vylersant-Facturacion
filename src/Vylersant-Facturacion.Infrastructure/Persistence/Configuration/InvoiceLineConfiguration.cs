using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vylersant_Facturacion.Domain.Entities.Invoices;
using Vylersant_Facturacion.Domain.Entities.Products;

namespace Vylersant_Facturacion.Infrastructure.Persistence.Configurations;

public class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("InvoiceLines");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        // Clave foránea oculta en la base de datos hacia Invoice (creada por la convención de la relación HasMany)
        builder.Property<Guid>("InvoiceId")
            .IsRequired();

        builder.Property(x => x.ProductId)
            .IsRequired();

        // REQUISITO: Relación con Product protegiendo el historial (Restrict)
        builder.HasOne<Product>() // Ajusta el namespace según corresponda
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.ProductName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Quantity)
            .HasPrecision(18,2)
            .IsRequired();

        builder.Property(x => x.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.LineTotal)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
    }
}