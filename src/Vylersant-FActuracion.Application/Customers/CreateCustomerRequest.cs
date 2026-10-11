namespace Vylersant_Facturacion.Application.Customers;

public sealed record CreateCustomerRequest(
    string Name,
    string? Address,
    string? Phone,
    string? Email);