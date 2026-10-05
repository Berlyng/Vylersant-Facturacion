using Vylersant_Facturacion.Domain.Entities.Users;

namespace Vylersant_Facturacion.Domain.Tests.Users
{
    public class UserTests
    {
        [Fact]
        public void Constructor_ShouldCreativeActiveUser_WhenDataIsValid()
        {
            // Arrange
            var businessId = Guid.NewGuid();


            // Act
            var user = new User(businessId, "John Doe", "john.doe@example.com", "hashedPassword", UserRole.Employee);


            // Assert
            Assert.NotEqual(Guid.Empty, user.Id);
            Assert.Equal(businessId, user.BusinessId);
            Assert.Equal("John Doe", user.Name);
            Assert.Equal("john.doe@example.com", user.Email);
            Assert.Equal("hashedPassword", user.PasswordHash);
            Assert.Equal(UserRole.Employee, user.Role);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenBusinessIdIsEmpty()
        {
            var action = () => new User(Guid.Empty, "John Doe", "john.doe@example.com", "hashedPassword", UserRole.Employee);
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenNameIsEmpty()
        {
            var action = () => new User(Guid.NewGuid(), "", "john.doe@example.com", "hashedPassword", UserRole.Employee);
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenEmailIsEmpty()
        {
            var action = () => new User(Guid.NewGuid(), "John Doe", "", "hashedPassword", UserRole.Employee);
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenPasswordHashIsEmpty()
        {
            var action = () => new User(Guid.NewGuid(), "John Doe", "john.doe@example.com", "", UserRole.Employee);
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Constructor_ShouldNormalizedEmail()
        {
            var user = new User(Guid.NewGuid(), "John Doe", "  JOHN.DOE@EXAMPLE.COM  ", "hashedPassword", UserRole.Employee);
            Assert.Equal("john.doe@example.com", user.Email);
        }

        [Fact]
        public void Activate_ShouldSetIsActiveToTrue()
        {
            var user = new User(Guid.NewGuid(), "John Doe", "john.doe@example.com", "hashedPassword", UserRole.Employee);
            user.Activate();
            Assert.True(user.IsActive);
        }

        [Fact]
        public void Deactivate_ShouldSetIsActiveToFalse()
        {
            var user = new User(Guid.NewGuid(), "John Doe", "john.doe@example.com", "hashedPassword", UserRole.Employee);
            user.Deactivate();
            Assert.False(user.IsActive);
        }

        [Fact]
        public void Constructor_Should_WhenEmailIsWhiteSpace()
        {
            var action = () => new User(Guid.NewGuid(), "John Doe", "   ", "hashedPassword", UserRole.Employee);
            Assert.Throws<ArgumentException>(action);

        }

        [Fact]
        public void Constructor_ShouldThrow_WhenRoleIsInvalid()
        {
            var invalidRole = (UserRole)999; // Assuming 999 is not a valid role
            var action = () => new User(Guid.NewGuid(), "John Doe", "john.doe@example.com", "hashedPassword", invalidRole);
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void ChangeRole_ShouldChangeRole_WhenRoleIsValid()
        {
            var user = new User(Guid.NewGuid(), "John Doe", "john.doe@example.com", "hashedPassword", UserRole.Employee);
            user.ChangeRole(UserRole.Supervisor);

            Assert.Equal(UserRole.Supervisor, user.Role);
        }   
    }
}
