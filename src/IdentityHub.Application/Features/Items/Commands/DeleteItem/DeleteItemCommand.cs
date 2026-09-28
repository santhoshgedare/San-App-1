using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Items.Commands.DeleteItem;

public sealed record DeleteItemCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteItemCommandHandler(IItemService itemService)
    : IRequestHandler<DeleteItemCommand, Result>
{
    public Task<Result> Handle(DeleteItemCommand request, CancellationToken cancellationToken)
        => itemService.DeleteAsync(request.Id, cancellationToken);
}
