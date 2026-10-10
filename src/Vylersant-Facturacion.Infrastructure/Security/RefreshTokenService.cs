using System.Security.Cryptography;
using System.Text;
using Vylersant_Facturacion.Application.Security;

namespace Vylersant_Facturacion.Infrastructure.Security;

public sealed class RefreshTokenService : IRefreshTokenService
{
    public string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);

        return Convert
            .ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var bytes =
            Encoding.UTF8.GetBytes(token);

        var hash =
            SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}