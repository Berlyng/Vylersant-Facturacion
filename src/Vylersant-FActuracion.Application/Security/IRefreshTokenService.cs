namespace Vylersant_Facturacion.Application.Security;

public interface IRefreshTokenService
{
    string Generate();

    string Hash(string token);
}