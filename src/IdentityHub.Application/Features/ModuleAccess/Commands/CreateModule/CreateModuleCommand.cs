using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.ModuleAccess.Commands.CreateModule;

public sealed record CreateModuleCommand(string Name, string Key, int SortOrder) : IRequest<Result<ModuleDto>>;

public sealed class CreateModuleCommandValidator : AbstractValidator<CreateModuleCommand>
{
    public CreateModuleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Key).NotEmpty().MaximumLength(150);
    }
}

public sealed class CreateModuleCommandHandler(IModuleAccessService moduleAccessService)
    : IRequestHandler<CreateModuleCommand, Result<ModuleDto>>
{
    public Task<Result<ModuleDto>> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
        => moduleAccessService.CreateModuleAsync(request.Name, request.Key, request.SortOrder, cancellationToken);
}
