namespace Vylersant_Facturacion.Application.Customers;

public sealed class CustomerNotFoundException : Exception
{
    public CustomerNotFoundException()
        : base("El cliente no fue encontrado.")
    {
    }
}