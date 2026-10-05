using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

[ApiController]
[Route("api/email-logs")]
[Authorize(Roles = Roles.Admin)]
public sealed class EmailLogsController(IEmailLogService logs) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<EmailLogDto>>> GetPaged(
        [FromQuery] string? status, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await logs.GetPagedAsync(new EmailLogQuery { Status = status, Search = search, Page = page, PageSize = pageSize }, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmailLogDto>> GetById(Guid id, CancellationToken ct)
    {
        var log = await logs.GetByIdAsync(id, ct);
        return log is null ? NotFound() : Ok(log);
    }
}
