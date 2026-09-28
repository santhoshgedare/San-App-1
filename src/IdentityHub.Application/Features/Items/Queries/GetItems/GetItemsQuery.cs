using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Items.Queries.GetItems;

public sealed record GetItemsQuery : IRequest<IReadOnlyList<ItemDto>>;

public sealed class GetItemsQueryHandler(IItemService itemService)
    : IRequestHandler<GetItemsQuery, IReadOnlyList<ItemDto>>
{
    public Task<IReadOnlyList<ItemDto>> Handle(GetItemsQuery request, CancellationToken cancellationToken)
        => itemService.GetAllAsync(cancellationToken);
}
