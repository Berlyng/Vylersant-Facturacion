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
    private readonly GetInvoicesService _getInvoicesService;
    private readonly GetInvoiceByIdService _getInvoiceByIdService;

    public InvoicesController(
        CreateInvoiceService createInvoiceService,
        GetInvoicesService getInvoicesService,
        GetInvoiceByIdService getInvoiceByIdService)
    {
        _createInvoiceService = createInvoiceService;
        _getInvoicesService = getInvoicesService;
        _getInvoiceByIdService = getInvoiceByIdService;
    }

    [HttpGet]
    public async Task<ActionResult<InvoicePageResponse>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(User.FindFirst(TokenClaimNames.BusinessId)?.Value, out var businessId))
        {
            return Unauthorized();
        }

        var result = await _getInvoicesService.ExecuteAsync(
            businessId,
            page,
            pageSize,
            cancellationToken);

        return Ok(new InvoicePageResponse(
            result.Items.Select(invoice => new InvoiceListItemResponse(
                invoice.Id,
                invoice.Number,
                invoice.IssuedAtUtc,
                invoice.Status.ToString(),
                invoice.CustomerId,
                invoice.Total)).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceDetailsResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirst(TokenClaimNames.BusinessId)?.Value, out var businessId))
        {
            return Unauthorized();
        }

        var invoice = await _getInvoiceByIdService.ExecuteAsync(id, businessId, cancellationToken);
        return Ok(new InvoiceDetailsResponse(
            invoice.Id,
            invoice.Number,
            invoice.IssuedAtUtc,
            invoice.Status.ToString(),
            invoice.CustomerId,
            invoice.CustomerName,
            invoice.Total,
            invoice.CancelledAtUtc,
            invoice.Lines.Select(line => new InvoiceLineResponse(
                line.ProductId,
                line.ProductName,
                line.Quantity,
                line.UnitPrice,
                line.LineTotal)).ToList()));
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

