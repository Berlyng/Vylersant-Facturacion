using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vylersant_Facturacion.Domain.Entities.Businesses;
using Vylersant_Facturacion.Domain.Entities.Products;

namespace Vylersant_Facturacion.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration
    : IEntityTypeConfiguration<Product>
{
    public void Configure(
        EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BusinessId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => x.BusinessId);

        builder.HasOne<Business>()
            .WithMany()
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}