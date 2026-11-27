using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vylersant_Facturacion.Application.Invoices;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Contracts.Invoices;
using Vylersant_Facturacion.Application.Payments;
using Vylersant_Facturacion.Contracts.Payments;
using Vylersant_Facturacion.Domain.Entities.Payments;
using RecordPaymentContractRequest = Vylersant_Facturacion.Contracts.Payments.RecordPaymentRequest;
using RecordPaymentApplicationRequest = Vylersant_Facturacion.Application.Payments.RecordPaymentRequest;

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
    private readonly CancelInvoiceService _cancelInvoiceService;
    private readonly RecordPaymentService _recordPaymentService;

    public InvoicesController(
        CreateInvoiceService createInvoiceService,
        GetInvoicesService getInvoicesService,
        GetInvoiceByIdService getInvoiceByIdService,
        CancelInvoiceService cancelInvoiceService,
        RecordPaymentService recordPaymentService)
    {
        _createInvoiceService = createInvoiceService;
        _getInvoicesService = getInvoicesService;
        _getInvoiceByIdService = getInvoiceByIdService;
        _cancelInvoiceService = cancelInvoiceService;
        _recordPaymentService = recordPaymentService;
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

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirst(TokenClaimNames.BusinessId)?.Value, out var businessId))
        {
            return Unauthorized();
        }

        await _cancelInvoiceService.ExecuteAsync(id, businessId, cancellationToken);
        return NoContent();
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

    [HttpPost("{id:guid}/payments")]
    public async Task<ActionResult<PaymentResponse>> RecordPayment(
        Guid id,
        RecordPaymentContractRequest request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirst(TokenClaimNames.BusinessId)?.Value, out var businessId))
            return Unauthorized();
        if (!Enum.TryParse<PaymentMethod>(request.Method, ignoreCase: true, out var method) || !Enum.IsDefined(method))
            return BadRequest("El método de pago no es válido.");

        var result = await _recordPaymentService.ExecuteAsync(
            id, businessId,
            new RecordPaymentApplicationRequest(request.Amount, method, request.Reference),
            cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new PaymentResponse(
            result.Id, result.Amount, result.PaidAtUtc, result.Method.ToString(), result.Reference,
            result.PaidTotal, result.Balance, result.IsPaid));
    }
}

