using System;
using System.Collections.Generic;
using System.Text;
using Vylersant_Facturacion.Infrastructure.Security;

namespace Vylersant_Facturacion.Infrastructure.Tests.Security
{
    public class BCryptPasswordHasherTests
    {
        private readonly BCryptPasswordHasher _passwordHasher = new();
        [Fact]
        public void Hash_ShouldNotReturnPlainTextPassword()
        {
            // Arrange
            const string password = "mySecurePassword";
            // Act
            var hashedPassword = _passwordHasher.Hash(password);
            // Assert
            Assert.NotEqual(password, hashedPassword);
        }
        [Fact]
        public void Verify_ShouldReturnTrue_WhenPasswordIsCorrect()
        {
            // Arrange
            const string password = "mySecurePassword";
            var hashedPassword = _passwordHasher.Hash(password);
            // Act
            var isVerified = _passwordHasher.Verify(password, hashedPassword);
            // Assert
            Assert.True(isVerified);
        }
        [Fact]
        public void Verify_ShouldReturnFalse_WhenPasswordIsIncorrect()
        {
            // Arrange
            const string password = "mySecurePassword";
            var hashedPassword = _passwordHasher.Hash(password);
            // Act
            var isVerified = _passwordHasher.Verify("wrongPassword", hashedPassword);
            // Assert
            Assert.False(isVerified);
        }

        [Fact]
        public void Hash_ShouldReturnDifferentHashes_ForSamePassword()
        {
            // Arrange
            const string password = "mySecurePassword";
            // Act
            var hashedPassword1 = _passwordHasher.Hash(password);
            var hashedPassword2 = _passwordHasher.Hash(password);
            // Assert
            Assert.NotEqual(hashedPassword1, hashedPassword2);
        }
    }
}
