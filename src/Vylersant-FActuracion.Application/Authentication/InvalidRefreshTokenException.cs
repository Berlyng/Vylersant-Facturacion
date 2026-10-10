namespace Vylersant_Facturacion.Application.Authentication;

public sealed class InvalidRefreshTokenException : Exception
{
    public InvalidRefreshTokenException()
        : base("El refresh token no es válido o ha expirado.")
    {
    }
}