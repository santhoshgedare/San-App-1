using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.ModuleAccess.Commands.CreatePage;

public sealed record CreatePageCommand(Guid ModuleId, string Name, string Url, int SortOrder) : IRequest<Result<PageDto>>;

public sealed class CreatePageCommandValidator : AbstractValidator<CreatePageCommand>
{
    public CreatePageCommandValidator()
    {
        RuleFor(x => x.ModuleId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public sealed class CreatePageCommandHandler(IModuleAccessService moduleAccessService)
    : IRequestHandler<CreatePageCommand, Result<PageDto>>
{
    public Task<Result<PageDto>> Handle(CreatePageCommand request, CancellationToken cancellationToken)
        => moduleAccessService.CreatePageAsync(request.ModuleId, request.Name, request.Url, request.SortOrder, cancellationToken);
}
