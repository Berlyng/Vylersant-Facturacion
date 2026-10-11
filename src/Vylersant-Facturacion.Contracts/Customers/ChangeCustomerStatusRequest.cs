namespace Vylersant_Facturacion.Contracts.Customers;

public sealed record ChangeCustomerStatusRequest(
    bool IsActive);