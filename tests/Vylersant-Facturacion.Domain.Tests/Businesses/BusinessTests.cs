using Xunit;
using Vylersant_Facturacion.Domain.Entities.Businesses;
namespace Vylersant_Facturacion.Domain.Tests.Businesses
{
    public class BusinessTests
    {
        [Fact]
        public void Constructor_ShouldCreateActiveBusiness_WhenNameIsValid()
        {
            // Arrange
            const string Name = "Negocio XYZ";

            // Act
            var business = new Business(Name);


            // Assert
            Assert.NotEqual(Guid.Empty, business.Id);
            Assert.Equal(Name, business.Name);
            Assert.True(business.IsActive);
        }

        [Fact]

        public void Constructor_ShouldThrow_WhenNameIsEmpty()
        {
            // Act
            var action = () => new Business("");
            // Assert
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Constructor_ShouldTrimName()
        {
            //Act
            var business = new Business("  Negocio XYZ  ");

            //Assert
            Assert.Equal("Negocio XYZ", business.Name);
        }

        [Fact]
        public void Deactivate_ShouldSetBusinessAsInactive()
        {
            // Arrange
            var business = new Business("Negocio XYZ");
            // Act
            business.Deactivate();
            // Assert
            Assert.False(business.IsActive);
        }

        [Fact]
        public void Activate_ShouldSetBusinessAsActive()
        {
            // Arrange
            var business = new Business("Negocio XYZ");
            business.Deactivate();
            // Act
            business.Activate();
            // Assert
            Assert.True(business.IsActive);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenNameIsOnlySpaces()
        {
            // Act
            var action = () => new Business("   ");
            // Assert
            Assert.Throws<ArgumentException>(action);
        }

    }
}
