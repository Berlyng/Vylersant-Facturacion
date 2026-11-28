using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vylersant_Facturacion.Application.Dashboard;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Contracts.Dashboard;

namespace Vylersant_Facturacion.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(GetDashboardReportService reportService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<DashboardReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DashboardReportResponse>> Get(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        [FromQuery] string groupBy = "day",
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(User.FindFirst(TokenClaimNames.BusinessId)?.Value, out var businessId))
            return Unauthorized();
        try
        {
            var report = await reportService.ExecuteAsync(businessId, fromUtc, toUtc, groupBy.ToLowerInvariant(), cancellationToken);
            return Ok(new DashboardReportResponse(report.FromUtc, report.ToUtc, report.GroupBy, report.SalesTotal,
                report.InvoiceCount, report.AverageTicket, report.PaymentsCollected, report.PreviousSalesTotal,
                report.PreviousInvoiceCount, report.SalesChangePercent,
                report.Trend.Select(point => new DashboardTrendPointResponse(point.PeriodStartUtc, point.SalesTotal,
                    point.InvoiceCount, point.PaymentsCollected)).ToList()));
        }
        catch (ArgumentException exception)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Rango de fechas inválido", detail: exception.Message);
        }
    }
}

