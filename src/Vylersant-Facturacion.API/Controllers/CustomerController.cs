using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vylersant_Facturacion.Application.Customers;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Contracts.Customers;

using CreateCustomerApplicationRequest =
    Vylersant_Facturacion.Application.Customers.CreateCustomerRequest;

using UpdateCustomerApplicationRequest =
    Vylersant_Facturacion.Application.Customers.UpdateCustomerRequest;

using CreateCustomerContractRequest =
    Vylersant_Facturacion.Contracts.Customers.CreateCustomerRequest;

using UpdateCustomerContractRequest =
    Vylersant_Facturacion.Contracts.Customers.UpdateCustomerRequest;

namespace Vylersant_Facturacion.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/customers")]
public sealed class CustomersController : ControllerBase
{
    private readonly CreateCustomerService _createCustomerService;
    private readonly GetCustomersService _getCustomersService;
    private readonly GetCustomerByIdService _getCustomerByIdService;
    private readonly UpdateCustomerService _updateCustomerService;
    private readonly ChangeCustomerStatusService _changeCustomerStatusService;

    public CustomersController(
        CreateCustomerService createCustomerService,
        GetCustomersService getCustomersService,
        GetCustomerByIdService getCustomerByIdService,
        UpdateCustomerService updateCustomerService,
        ChangeCustomerStatusService changeCustomerStatusService)
    {
        _createCustomerService = createCustomerService;
        _getCustomersService = getCustomersService;
        _getCustomerByIdService = getCustomerByIdService;
        _updateCustomerService = updateCustomerService;
        _changeCustomerStatusService = changeCustomerStatusService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateCustomerResponse>> Create(
        CreateCustomerContractRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        if (businessId is null)
        {
            return Unauthorized();
        }

        var applicationRequest =
            new CreateCustomerApplicationRequest(
                request.Name,
                request.Address,
                request.Phone,
                request.Email);

        var result =
            await _createCustomerService.ExecuteAsync(
                businessId.Value,
                applicationRequest,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.CustomerId },
            new CreateCustomerResponse(
                result.CustomerId));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        if (businessId is null)
        {
            return Unauthorized();
        }

        var customers =
            await _getCustomersService.ExecuteAsync(
                businessId.Value,
                cancellationToken);

        var response =
            customers
                .Select(customer =>
                    new CustomerResponse(
                        customer.Id,
                        customer.Name,
                        customer.Address,
                        customer.Phone,
                        customer.Email,
                        customer.IsActive))
                .ToList();

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        if (businessId is null)
        {
            return Unauthorized();
        }

        var customer =
            await _getCustomerByIdService.ExecuteAsync(
                id,
                businessId.Value,
                cancellationToken);

        return Ok(
            new CustomerResponse(
                customer.Id,
                customer.Name,
                customer.Address,
                customer.Phone,
                customer.Email,
                customer.IsActive));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateCustomerContractRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        if (businessId is null)
        {
            return Unauthorized();
        }

        var applicationRequest =
            new UpdateCustomerApplicationRequest(
                request.Name,
                request.Address,
                request.Phone,
                request.Email);

        await _updateCustomerService.ExecuteAsync(
            id,
            businessId.Value,
            applicationRequest,
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        ChangeCustomerStatusRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        if (businessId is null)
        {
            return Unauthorized();
        }

        await _changeCustomerStatusService.ExecuteAsync(
            id,
            businessId.Value,
            request.IsActive,
            cancellationToken);

        return NoContent();
    }

    private Guid? GetBusinessId()
    {
        var businessIdClaim =
            User.FindFirst(
                TokenClaimNames.BusinessId)?.Value;

        if (!Guid.TryParse(
                businessIdClaim,
                out var businessId))
        {
            return null;
        }

        return businessId;
    }
}