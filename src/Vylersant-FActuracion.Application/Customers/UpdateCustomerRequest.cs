namespace Vylersant_Facturacion.Application.Customers;

public sealed record UpdateCustomerRequest(
    string Name,
    string? Address,
    string? Phone,
    string? Email);