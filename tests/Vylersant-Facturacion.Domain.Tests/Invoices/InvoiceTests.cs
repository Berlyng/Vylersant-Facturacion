using Vylersant_Facturacion.Domain.Entities.Invoices;

namespace Vylersant_Facturacion.Domain.Tests.Invoices;

public class InvoiceTests
{
    [Fact]
    public void Constructor_ShouldCreateInvoice_WhenDataIsValid()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var lines = new[]
        {
            new InvoiceLine(
                Guid.NewGuid(),
                "Arroz",
                2m,
                150m),

            new InvoiceLine(
                Guid.NewGuid(),
                "Aceite",
                1m,
                250m)
        };

        var issuedAtUtc = DateTime.UtcNow;

        // Act
        var invoice = new Invoice(
            businessId,
            customerId,
            1,
            issuedAtUtc,
            lines);

        // Assert
        Assert.NotEqual(
            Guid.Empty,
            invoice.Id);

        Assert.Equal(
            businessId,
            invoice.BusinessId);

        Assert.Equal(
            customerId,
            invoice.CustomerId);

        Assert.Equal(
            1,
            invoice.Number);

        Assert.Equal(
            InvoiceStatus.Issued,
            invoice.Status);

        Assert.Equal(
            2,
            invoice.Lines.Count);

        Assert.Equal(
            550m,
            invoice.Total);
    }

    [Fact]
    public void Constructor_ShouldAllowInvoiceWithoutCustomer()
    {
        var invoice = new Invoice(
            Guid.NewGuid(),
            null,
            1,
            DateTime.UtcNow,
            [
                new InvoiceLine(
                    Guid.NewGuid(),
                    "Producto",
                    1m,
                    100m)
            ]);

        Assert.Null(invoice.CustomerId);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenBusinessIdIsEmpty()
    {
        var action = () =>
            new Invoice(
                Guid.Empty,
                null,
                1,
                DateTime.UtcNow,
                [
                    new InvoiceLine(
                        Guid.NewGuid(),
                        "Producto",
                        1m,
                        100m)
                ]);

        Assert.Throws<ArgumentException>(
            action);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenCustomerIdIsEmpty()
    {
        var action = () =>
            new Invoice(
                Guid.NewGuid(),
                Guid.Empty,
                1,
                DateTime.UtcNow,
                [
                    new InvoiceLine(
                        Guid.NewGuid(),
                        "Producto",
                        1m,
                        100m)
                ]);

        Assert.Throws<ArgumentException>(
            action);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNumberIsInvalid()
    {
        var action = () =>
            new Invoice(
                Guid.NewGuid(),
                null,
                0,
                DateTime.UtcNow,
                [
                    new InvoiceLine(
                        Guid.NewGuid(),
                        "Producto",
                        1m,
                        100m)
                ]);

        Assert.Throws<ArgumentOutOfRangeException>(
            action);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenInvoiceHasNoLines()
    {
        var action = () =>
            new Invoice(
                Guid.NewGuid(),
                null,
                1,
                DateTime.UtcNow,
                []);

        Assert.Throws<ArgumentException>(
            action);
    }

    [Fact]
    public void Cancel_ShouldMarkInvoiceAsCancelled()
    {
        // Arrange
        var invoice = CreateInvoice();

        var cancelledAt =
            DateTime.UtcNow;

        // Act
        invoice.Cancel(cancelledAt);

        // Assert
        Assert.Equal(
            InvoiceStatus.Cancelled,
            invoice.Status);

        Assert.Equal(
            cancelledAt,
            invoice.CancelledAtUtc);
    }

    private static Invoice CreateInvoice()
    {
        return new Invoice(
            Guid.NewGuid(),
            null,
            1,
            DateTime.UtcNow,
            [
                new InvoiceLine(
                    Guid.NewGuid(),
                    "Producto",
                    1m,
                    100m)
            ]);
    }
}