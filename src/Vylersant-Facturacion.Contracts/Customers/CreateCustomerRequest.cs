namespace Vylersant_Facturacion.Contracts.Customers;

public sealed record CreateCustomerRequest(
    string Name,
    string? Address,
    string? Phone,
    string? Email);