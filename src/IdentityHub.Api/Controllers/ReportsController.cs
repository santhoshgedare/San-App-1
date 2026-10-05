using IdentityHub.Api.Authorization;
using IdentityHub.Application.Common.Models;
using IdentityHub.Application.Features.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public sealed class ReportsController(ISender sender) : ControllerBase
{
    /// <summary>Gets paid sales, refunds, cost of goods sold, and gross profit/loss for a date range.</summary>
    [HttpGet("profit-loss")]
    [RequireSection("section-reports-view")]
    [ProducesResponseType<ProfitLossReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProfitLossReportDto>> GetProfitLoss(
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = startDate ?? new DateOnly(today.Year, today.Month, 1);
        var to = endDate ?? today;

        if (from > to)
        {
            return BadRequest(new { error = "Start date must be on or before end date." });
        }
        if (to.DayNumber - from.DayNumber > 365)
        {
            return BadRequest(new { error = "The report date range cannot exceed 366 days." });
        }

        var report = await sender.Send(new GetProfitLossReportQuery(from, to), ct);
        return Ok(report);
    }
}