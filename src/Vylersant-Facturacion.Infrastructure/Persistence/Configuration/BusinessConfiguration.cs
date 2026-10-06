using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Vylersant_Facturacion.Domain.Entities.Businesses;

namespace Vylersant_Facturacion.Infrastructure.Persistence.Configuration
{
    public sealed class BusinessConfiguration : IEntityTypeConfiguration<Business>
    {
        public void Configure(EntityTypeBuilder<Business> builder)
        {
            builder.ToTable("Businesses");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(150);
            builder.Property(b => b.Address)
                .HasMaxLength(250);
            builder.Property(b => b.PhoneNumber)
                .HasMaxLength(30);
            builder.Property(b => b.Email)
                .HasMaxLength(150);
            builder.Property(b => b.IsActive)
                .IsRequired();
        }
    }
}
