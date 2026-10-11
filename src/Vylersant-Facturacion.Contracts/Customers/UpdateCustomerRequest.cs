namespace Vylersant_Facturacion.Contracts.Customers;

public sealed record UpdateCustomerRequest(
    string Name,
    string? Address,
    string? Phone,
    string? Email);