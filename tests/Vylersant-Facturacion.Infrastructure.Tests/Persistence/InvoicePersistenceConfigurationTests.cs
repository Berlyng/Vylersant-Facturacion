using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Vylersant_Facturacion.Domain.Entities.Invoices;
using Vylersant_Facturacion.Infrastructure.Persistence;
using Xunit;

namespace Vylersant_Facturacion.Infrastructure.Tests.Persistence;

public sealed class InvoicePersistenceConfigurationTests
{
    [Fact]
    public void Model_ShouldConfigureInvoiceAndInvoiceLine()
    {
        var options = new DbContextOptionsBuilder<VylersantFacturacionDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;" +
                "Database=VylersantModelValidation;" +
                "Trusted_Connection=True;" +
                "TrustServerCertificate=True;")
            .Options;

        using var context = new VylersantFacturacionDbContext(options);

        var invoice = context.Model.FindEntityType(typeof(Invoice));
        var line = context.Model.FindEntityType(typeof(InvoiceLine));

        Assert.NotNull(invoice);
        Assert.NotNull(line);

        Assert.Equal("Invoices", invoice!.GetTableName());
        Assert.Equal("InvoiceLines", line!.GetTableName());

        var total = invoice.FindProperty(nameof(Invoice.Total));

        Assert.NotNull(total);
        Assert.Equal(18, total!.GetPrecision());
        Assert.Equal(2, total.GetScale());

        var quantity = line.FindProperty(nameof(InvoiceLine.Quantity));

        Assert.NotNull(quantity);
        Assert.Equal(18, quantity!.GetPrecision());
        Assert.Equal(2, quantity.GetScale());

        var invoiceNumberIndex = invoice.GetIndexes()
            .SingleOrDefault(index =>
                index.IsUnique &&
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(new[]
                    {
                        nameof(Invoice.BusinessId),
                        nameof(Invoice.Number)
                    }));

        Assert.NotNull(invoiceNumberIndex);

        var businessForeignKey = invoice.GetForeignKeys()
            .SingleOrDefault(foreignKey =>
                foreignKey.Properties.Any(property =>
                    property.Name == nameof(Invoice.BusinessId)));

        Assert.NotNull(businessForeignKey);
        Assert.True(businessForeignKey!.IsRequired);
        Assert.Equal(
            DeleteBehavior.Restrict,
            businessForeignKey.DeleteBehavior);

        var customerForeignKey = invoice.GetForeignKeys()
            .SingleOrDefault(foreignKey =>
                foreignKey.Properties.Any(property =>
                    property.Name == nameof(Invoice.CustomerId)));

        Assert.NotNull(customerForeignKey);
        Assert.False(customerForeignKey!.IsRequired);
        Assert.Equal(
            DeleteBehavior.Restrict,
            customerForeignKey.DeleteBehavior);

        var invoiceLineForeignKey = line.GetForeignKeys()
            .SingleOrDefault(foreignKey =>
                foreignKey.Properties.Any(property =>
                    property.Name == "InvoiceId"));

        Assert.NotNull(invoiceLineForeignKey);
        Assert.Equal(
            DeleteBehavior.Restrict,
            invoiceLineForeignKey!.DeleteBehavior);
    }
}