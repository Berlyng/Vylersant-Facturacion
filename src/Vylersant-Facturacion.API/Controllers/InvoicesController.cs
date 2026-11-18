using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vylersant_Facturacion.Application.Invoices;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Contracts.Invoices;

using CreateInvoiceApplicationRequest = Vylersant_Facturacion.Application.Invoices.CreateInvoiceRequest;
using CreateInvoiceLineApplicationRequest = Vylersant_Facturacion.Application.Invoices.CreateInvoiceLineRequest;
using CreateInvoiceContractRequest = Vylersant_Facturacion.Contracts.Invoices.CreateInvoiceRequest;

namespace Vylersant_Facturacion.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/invoices")]
public sealed class InvoicesController : ControllerBase
{
    private readonly CreateInvoiceService _createInvoiceService;

    public InvoicesController(CreateInvoiceService createInvoiceService)
    {
        _createInvoiceService = createInvoiceService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateInvoiceResponse>> Create(
        CreateInvoiceContractRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Lines is null || request.Lines.Any(line => line is null))
        {
            return BadRequest("La factura debe contener al menos una línea.");
        }

        if (!Guid.TryParse(User.FindFirst(TokenClaimNames.BusinessId)?.Value, out var businessId) ||
            !Guid.TryParse(User.FindFirst(TokenClaimNames.UserId)?.Value, out var userId))
        {
            return Unauthorized();
        }

        var applicationRequest = new CreateInvoiceApplicationRequest(
            request.CustomerId,
            request.Lines
                .Select(line => new CreateInvoiceLineApplicationRequest(line.ProductId, line.Quantity))
                .ToList());

        var result = await _createInvoiceService.ExecuteAsync(
            businessId,
            userId,
            applicationRequest,
            cancellationToken);

        var response = new CreateInvoiceResponse(result.InvoiceId, result.Number, result.Total);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}

