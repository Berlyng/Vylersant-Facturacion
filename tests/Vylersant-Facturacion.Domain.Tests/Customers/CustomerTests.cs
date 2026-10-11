using Vylersant_Facturacion.Domain.Entities.Customers;

namespace Vylersant_Facturacion.Domain.Tests.Customers;

public class CustomerTests
{
    [Fact]
    public void Constructor_ShouldCreateCustomer_WhenDataIsValid()
    {
        // Arrange
        var businessId =
            Guid.NewGuid();

        // Act
        var customer =
            new Customer(
                businessId,
                "Juan Pérez",
                "Santo Domingo",
                "809-555-1234",
                "JUAN@EMAIL.COM");

        // Assert
        Assert.NotEqual(
            Guid.Empty,
            customer.Id);

        Assert.Equal(
            businessId,
            customer.BusinessId);

        Assert.Equal(
            "Juan Pérez",
            customer.Name);

        Assert.Equal(
            "Santo Domingo",
            customer.Address);

        Assert.Equal(
            "809-555-1234",
            customer.Phone);

        Assert.Equal(
            "juan@email.com",
            customer.Email);

        Assert.True(
            customer.IsActive);
    }

    [Fact]
    public void Constructor_ShouldTrimName()
    {
        var customer =
            new Customer(
                Guid.NewGuid(),
                "  Juan Pérez  ");

        Assert.Equal(
            "Juan Pérez",
            customer.Name);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenBusinessIdIsEmpty()
    {
        var action = () =>
            new Customer(
                Guid.Empty,
                "Juan Pérez");

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
            new Customer(
                Guid.NewGuid(),
                name);

        Assert.Throws<ArgumentException>(
            action);
    }

    [Fact]
    public void Constructor_ShouldConvertOptionalWhitespaceToNull()
    {
        var customer =
            new Customer(
                Guid.NewGuid(),
                "Juan Pérez",
                "   ",
                "   ",
                "   ");

        Assert.Null(
            customer.Address);

        Assert.Null(
            customer.Phone);

        Assert.Null(
            customer.Email);
    }

    [Fact]
    public void ChangeName_ShouldUpdateAndTrimName()
    {
        var customer =
            CreateCustomer();

        customer.ChangeName(
            "  Pedro Martínez  ");

        Assert.Equal(
            "Pedro Martínez",
            customer.Name);
    }

    [Fact]
    public void ChangeName_ShouldThrowException_WhenNameIsEmpty()
    {
        var customer =
            CreateCustomer();

        var action = () =>
            customer.ChangeName(" ");

        Assert.Throws<ArgumentException>(
            action);
    }

    [Fact]
    public void ChangeAddress_ShouldUpdateAddress()
    {
        var customer =
            CreateCustomer();

        customer.ChangeAddress(
            "Santiago");

        Assert.Equal(
            "Santiago",
            customer.Address);
    }

    [Fact]
    public void ChangePhone_ShouldUpdatePhone()
    {
        var customer =
            CreateCustomer();

        customer.ChangePhone(
            "809-555-9999");

        Assert.Equal(
            "809-555-9999",
            customer.Phone);
    }

    [Fact]
    public void ChangeEmail_ShouldNormalizeEmail()
    {
        var customer =
            CreateCustomer();

        customer.ChangeEmail(
            "  NUEVO@EMAIL.COM  ");

        Assert.Equal(
            "nuevo@email.com",
            customer.Email);
    }

    [Fact]
    public void Deactivate_ShouldMarkCustomerAsInactive()
    {
        var customer =
            CreateCustomer();

        customer.Deactivate();

        Assert.False(
            customer.IsActive);
    }

    [Fact]
    public void Activate_ShouldMarkCustomerAsActive()
    {
        var customer =
            CreateCustomer();

        customer.Deactivate();

        customer.Activate();

        Assert.True(
            customer.IsActive);
    }

    private static Customer CreateCustomer()
    {
        return new Customer(
            Guid.NewGuid(),
            "Juan Pérez",
            "Santo Domingo",
            "809-555-1234",
            "juan@email.com");
    }
}