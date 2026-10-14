using IdentityHub.Api.Authorization;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

public sealed record CustomRequestImageRequest(string Url, string? FileName);

public sealed record CreateCustomRequestRequest(
    string Title,
    string Description,
    int Quantity,
    decimal? Budget,
    DateOnly? NeededBy,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    List<CustomRequestImageRequest>? Images);

public sealed record CustomRequestInterestRequest(string? Message);

public sealed record CustomRequestMessageRequest(string Body);

public sealed record CustomRequestQuoteRequest(decimal UnitPrice, decimal? ShippingFee, int? LeadTimeDays, string? Note);

public sealed record AwardCustomRequestRequest(string ShippingAddress, string? OrderNotes);

public sealed record OrderProgressRequest(string Stage, string? Note, string? ImageUrl);

/// <summary>Custom design requests: customers post them, sellers quote, the customer awards one quote as an order.</summary>
[ApiController]
[Route("api/custom-requests")]
[Authorize]
public sealed class CustomRequestsController(ICustomRequestService requests) : ControllerBase
{
    [HttpGet]
    [RequireSection("section-custom-requests-view")]
    public async Task<ActionResult<PagedResult<CustomRequestSummaryDto>>> GetAll(
        [FromQuery] string? scope, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await requests.GetPagedAsync(scope, status, page, pageSize, ct));

    [HttpGet("{id:guid}")]
    [RequireSection("section-custom-requests-view")]
    public async Task<ActionResult<CustomRequestDetailDto>> Get(Guid id, CancellationToken ct)
    {
        var detail = await requests.GetAsync(id, ct);
        return detail is null ? NotFound() : Ok(detail);
    }

    [HttpPost]
    [RequireSection("section-custom-requests-view")]
    public async Task<ActionResult<CustomRequestDetailDto>> Create(CreateCustomRequestRequest request, CancellationToken ct)
    {
        var result = await requests.CreateAsync(new CreateCustomRequestInput(
            request.Title, request.Description, request.Quantity, request.Budget, request.NeededBy,
            request.CustomerName, request.CustomerEmail, request.CustomerPhone,
            (request.Images ?? []).Select(i => new CustomRequestImageInput(i.Url, i.FileName)).ToList()), ct);
        return result.Succeeded ? CreatedAtAction(nameof(Get), new { id = result.Data!.Id }, result.Data) : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("{id:guid}/cancel")]
    [RequireSection("section-custom-requests-view")]
    public async Task<ActionResult<CustomRequestDetailDto>> Cancel(Guid id, CancellationToken ct)
        => ToResponse(await requests.CancelAsync(id, ct));

    [HttpPost("{id:guid}/interest")]
    [RequireSection("section-custom-requests-quote")]
    public async Task<ActionResult<CustomRequestDetailDto>> ExpressInterest(Guid id, CustomRequestInterestRequest request, CancellationToken ct)
        => ToResponse(await requests.ExpressInterestAsync(id, request.Message, ct));

    [HttpPost("{id:guid}/threads/{threadId:guid}/messages")]
    [RequireSection("section-custom-requests-view")]
    public async Task<ActionResult<CustomRequestDetailDto>> SendMessage(Guid id, Guid threadId, CustomRequestMessageRequest request, CancellationToken ct)
        => ToResponse(await requests.SendMessageAsync(id, threadId, request.Body, ct));

    [HttpPut("{id:guid}/threads/{threadId:guid}/quote")]
    [RequireSection("section-custom-requests-quote")]
    public async Task<ActionResult<CustomRequestDetailDto>> SubmitQuote(Guid id, Guid threadId, CustomRequestQuoteRequest request, CancellationToken ct)
        => ToResponse(await requests.SubmitQuoteAsync(id, threadId, new CustomQuoteInput(request.UnitPrice, request.ShippingFee, request.LeadTimeDays, request.Note), ct));

    [HttpPost("{id:guid}/threads/{threadId:guid}/withdraw")]
    [RequireSection("section-custom-requests-quote")]
    public async Task<ActionResult<CustomRequestDetailDto>> Withdraw(Guid id, Guid threadId, CancellationToken ct)
        => ToResponse(await requests.WithdrawAsync(id, threadId, ct));

    /// <summary>The customer accepts a seller's quote; creates the order and returns its id.</summary>
    [HttpPost("{id:guid}/threads/{threadId:guid}/award")]
    [RequireSection("section-custom-requests-view")]
    public async Task<ActionResult<object>> Award(Guid id, Guid threadId, AwardCustomRequestRequest request, CancellationToken ct)
    {
        var result = await requests.AwardAsync(id, threadId, new AwardCustomRequestInput(request.ShippingAddress, request.OrderNotes), ct);
        return result.Succeeded ? Ok(new { orderId = result.Data }) : BadRequest(new { errors = result.Errors });
    }

    /// <summary>The seller of a custom order posts a production stage with an optional note and photo.</summary>
    [HttpPost("orders/{orderId:guid}/progress")]
    [RequireSection("section-custom-requests-quote")]
    public async Task<IActionResult> AddProgress(Guid orderId, OrderProgressRequest request, CancellationToken ct)
    {
        var result = await requests.AddProgressAsync(orderId, request.Stage, request.Note, request.ImageUrl, ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    private ActionResult<CustomRequestDetailDto> ToResponse(Result<CustomRequestDetailDto> result)
        => result.Succeeded ? Ok(result.Data) : BadRequest(new { errors = result.Errors });
}
