using System;
using System.Collections.Generic;
using System.Text;
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
            var user = new User(businessId, "John Doe", "john.doe@example.com", "hashedPassword");


            // Assert
            Assert.NotEqual(Guid.Empty, user.Id);
            Assert.Equal(businessId, user.BusinessId);
            Assert.Equal("John Doe", user.Name);
            Assert.Equal("john.doe@example.com", user.Email);
            Assert.Equal("hashedPassword", user.PasswordHash);

        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenBusinessIdIsEmpty()
        {
            var action = () => new User(Guid.Empty, "John Doe", "john.doe@example.com", "hashedPassword");
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenNameIsEmpty()
        {
            var action = () => new User(Guid.NewGuid(), "", "john.doe@example.com", "hashedPassword");
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenEmailIsEmpty()
        {
            var action = () => new User(Guid.NewGuid(), "John Doe", "", "hashedPassword");
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenPasswordHashIsEmpty()
        {
            var action = () => new User(Guid.NewGuid(), "John Doe", "john.doe@example.com", "");
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Constructor_ShouldNormalizedEmail()
        {
            var user = new User(Guid.NewGuid(), "John Doe", "  JOHN.DOE@EXAMPLE.COM  ", "hashedPassword");
            Assert.Equal("john.doe@example.com", user.Email);
        }

        [Fact]
        public void Activate_ShouldSetIsActiveToTrue()
        {
            var user = new User(Guid.NewGuid(), "John Doe", "john.doe@example.com", "hashedPassword");
            user.Activate();
            Assert.True(user.IsActive);
        }

        [Fact]
        public void Deactivate_ShouldSetIsActiveToFalse()
        {
            var user = new User(Guid.NewGuid(), "John Doe", "john.doe@example.com", "hashedPassword");
            user.Deactivate();
            Assert.False(user.IsActive);
        }

        [Fact]
        public void Constructor_Should_WhenEmailIsWhiteSpace()
        {
            var action = () => new User(Guid.NewGuid(), "John Doe", "   ", "hashedPassword");
            Assert.Throws<ArgumentException>(action);

        }
    }
}
