using Microsoft.EntityFrameworkCore;
using Vylersant_Facturacion.Domain.Entities.Payments;
using Vylersant_Facturacion.Infrastructure.Persistence;

namespace Vylersant_Facturacion.Infrastructure.Tests.Persistence;

public sealed class PaymentPersistenceConfigurationTests
{
    [Fact]
    public void Model_ShouldConfigurePaymentStorageAndTenantIndex()
    {
        var options = new DbContextOptionsBuilder<VylersantFacturacionDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=VylersantPaymentModel;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;
        using var context = new VylersantFacturacionDbContext(options);

        var payment = context.Model.FindEntityType(typeof(Payment));
        Assert.NotNull(payment);
        Assert.Equal("Payments", payment!.GetTableName());
        Assert.Equal(18, payment.FindProperty(nameof(Payment.Amount))!.GetPrecision());
        Assert.Equal(2, payment.FindProperty(nameof(Payment.Amount))!.GetScale());
        Assert.Equal(500, payment.FindProperty(nameof(Payment.VoidReason))!.GetMaxLength());
        Assert.Contains(payment.GetIndexes(), index => index.Properties.Select(property => property.Name)
            .SequenceEqual(new[] { nameof(Payment.BusinessId), nameof(Payment.InvoiceId), nameof(Payment.Status) }));
        Assert.Contains(payment.GetForeignKeys(), foreignKey => foreignKey.Properties.Any(property => property.Name == nameof(Payment.InvoiceId))
            && foreignKey.DeleteBehavior == DeleteBehavior.Restrict);
    }
}

