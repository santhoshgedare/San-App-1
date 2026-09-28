using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Items.Queries.GetItemById;

public sealed record GetItemByIdQuery(Guid Id) : IRequest<ItemDto?>;

public sealed class GetItemByIdQueryHandler(IItemService itemService)
    : IRequestHandler<GetItemByIdQuery, ItemDto?>
{
    public Task<ItemDto?> Handle(GetItemByIdQuery request, CancellationToken cancellationToken)
        => itemService.GetByIdAsync(request.Id, cancellationToken);
}
