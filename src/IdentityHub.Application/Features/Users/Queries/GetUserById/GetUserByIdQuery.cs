using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;

public sealed class GetUserByIdQueryHandler(IIdentityService identityService) : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    public Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        => identityService.FindByIdAsync(request.UserId, cancellationToken);
}
