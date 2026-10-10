using Vylersant_Facturacion.Domain.Entities.Products;

namespace Vylersant_Facturacion.Domain.Tests.Products;

public class ProductTests
{
    [Fact]
    public void Constructor_ShouldCreateProduct_WhenDataIsValid()
    {
        // Arrange
        var businessId = Guid.NewGuid();

        // Act
        var product = new Product(
            businessId,
            "Arroz Premium",
            125.50m,
            "ARR-001",
            "Saco de arroz premium");

        // Assert
        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal(businessId, product.BusinessId);
        Assert.Equal("Arroz Premium", product.Name);
        Assert.Equal(125.50m, product.Price);
        Assert.Equal("ARR-001", product.Sku);
        Assert.Equal(
            "Saco de arroz premium",
            product.Description);
        Assert.True(product.IsActive);
    }

    [Fact]
    public void Constructor_ShouldTrimName()
    {
        var product = new Product(
            Guid.NewGuid(),
            "  Arroz Premium  ",
            100m);

        Assert.Equal(
            "Arroz Premium",
            product.Name);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenBusinessIdIsEmpty()
    {
        var action = () =>
            new Product(
                Guid.Empty,
                "Producto",
                100m);

        Assert.Throws<ArgumentException>(
            action);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowException_WhenNameIsInvalid(
        string name)
    {
        var action = () =>
            new Product(
                Guid.NewGuid(),
                name,
                100m);

        Assert.Throws<ArgumentException>(
            action);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenPriceIsNegative()
    {
        var action = () =>
            new Product(
                Guid.NewGuid(),
                "Producto",
                -1m);

        Assert.Throws<ArgumentOutOfRangeException>(
            action);
    }

    [Fact]
    public void Constructor_ShouldAllowZeroPrice()
    {
        var product = new Product(
            Guid.NewGuid(),
            "Producto gratis",
            0m);

        Assert.Equal(
            0m,
            product.Price);
    }

    [Fact]
    public void ChangePrice_ShouldUpdatePrice_WhenPriceIsValid()
    {
        var product = CreateProduct();

        product.ChangePrice(250m);

        Assert.Equal(
            250m,
            product.Price);
    }

    [Fact]
    public void ChangePrice_ShouldThrowException_WhenPriceIsNegative()
    {
        var product = CreateProduct();

        var action = () =>
            product.ChangePrice(-10m);

        Assert.Throws<ArgumentOutOfRangeException>(
            action);
    }

    [Fact]
    public void ChangeName_ShouldUpdateAndTrimName()
    {
        var product = CreateProduct();

        product.ChangeName(
            "  Nuevo producto  ");

        Assert.Equal(
            "Nuevo producto",
            product.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ChangeName_ShouldThrowException_WhenNameIsInvalid(
        string name)
    {
        var product = CreateProduct();

        var action = () =>
            product.ChangeName(name);

        Assert.Throws<ArgumentException>(
            action);
    }

    [Fact]
    public void OptionalFields_ShouldBecomeNull_WhenWhitespaceIsProvided()
    {
        var product = new Product(
            Guid.NewGuid(),
            "Producto",
            100m,
            "   ",
            "   ");

        Assert.Null(product.Sku);
        Assert.Null(product.Description);
    }

    [Fact]
    public void Deactivate_ShouldMarkProductAsInactive()
    {
        var product = CreateProduct();

        product.Deactivate();

        Assert.False(product.IsActive);
    }

    [Fact]
    public void Activate_ShouldMarkProductAsActive()
    {
        var product = CreateProduct();

        product.Deactivate();
        product.Activate();

        Assert.True(product.IsActive);
    }

    private static Product CreateProduct()
    {
        return new Product(
            Guid.NewGuid(),
            "Producto",
            100m);
    }
}