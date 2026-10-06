using Vylersant_Facturacion.Application.Security;
using BC = BCrypt.Net.BCrypt;

namespace Vylersant_Facturacion.Infrastructure.Security
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(password);
            return BC.HashPassword(password);
        }

        public bool Verify(string password, string hashedPassword)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(password);
            ArgumentException.ThrowIfNullOrWhiteSpace(hashedPassword);

            return BC.Verify(password, hashedPassword);
        }
    }
}
