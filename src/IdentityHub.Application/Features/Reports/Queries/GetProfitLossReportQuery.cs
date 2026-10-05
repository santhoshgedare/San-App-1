using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Reports.Queries;

public sealed record GetProfitLossReportQuery(DateOnly StartDate, DateOnly EndDate)
    : IRequest<ProfitLossReportDto>;

public sealed class GetProfitLossReportQueryHandler(IOrderService orderService)
    : IRequestHandler<GetProfitLossReportQuery, ProfitLossReportDto>
{
    public Task<ProfitLossReportDto> Handle(GetProfitLossReportQuery request, CancellationToken ct)
    {
        var startInclusive = new DateTimeOffset(request.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        var endExclusive = new DateTimeOffset(request.EndDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

        return orderService.GetProfitLossReportAsync(
            startInclusive,
            endExclusive,
            request.StartDate,
            request.EndDate,
            ct);
    }
}