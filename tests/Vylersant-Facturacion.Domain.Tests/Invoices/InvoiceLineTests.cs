using Vylersant_Facturacion.Domain.Entities.Invoices;

namespace Vylersant_Facturacion.Domain.Tests.Invoices;

public class InvoiceLineTests
{
    [Fact]
    public void Constructor_ShouldCalculateLineTotal()
    {
        var line = new InvoiceLine(
            Guid.NewGuid(),
            "Arroz",
            2.5m,
            100m);

        Assert.Equal(
            250m,
            line.LineTotal);
    }

    [Fact]
    public void Constructor_ShouldPreserveProductSnapshot()
    {
        var productId =
            Guid.NewGuid();

        var line = new InvoiceLine(
            productId,
            "Arroz Premium",
            2m,
            150m);

        Assert.Equal(
            productId,
            line.ProductId);

        Assert.Equal(
            "Arroz Premium",
            line.ProductName);

        Assert.Equal(
            150m,
            line.UnitPrice);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenProductIdIsEmpty()
    {
        var action = () =>
            new InvoiceLine(
                Guid.Empty,
                "Producto",
                1m,
                100m);

        Assert.Throws<ArgumentException>(
            action);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenQuantityIsZero()
    {
        var action = () =>
            new InvoiceLine(
                Guid.NewGuid(),
                "Producto",
                0m,
                100m);

        Assert.Throws<ArgumentOutOfRangeException>(
            action);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenQuantityIsNegative()
    {
        var action = () =>
            new InvoiceLine(
                Guid.NewGuid(),
                "Producto",
                -1m,
                100m);

        Assert.Throws<ArgumentOutOfRangeException>(
            action);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenUnitPriceIsNegative()
    {
        var action = () =>
            new InvoiceLine(
                Guid.NewGuid(),
                "Producto",
                1m,
                -1m);

        Assert.Throws<ArgumentOutOfRangeException>(
            action);
    }
}