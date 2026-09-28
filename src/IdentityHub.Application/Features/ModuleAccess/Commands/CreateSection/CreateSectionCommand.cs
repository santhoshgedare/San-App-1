using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.ModuleAccess.Commands.CreateSection;

public sealed record CreateSectionCommand(Guid PageId, string Name, string Key, int SortOrder) : IRequest<Result<SectionDto>>;

public sealed class CreateSectionCommandValidator : AbstractValidator<CreateSectionCommand>
{
    public CreateSectionCommandValidator()
    {
        RuleFor(x => x.PageId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Key).NotEmpty().MaximumLength(150);
    }
}

public sealed class CreateSectionCommandHandler(IModuleAccessService moduleAccessService)
    : IRequestHandler<CreateSectionCommand, Result<SectionDto>>
{
    public Task<Result<SectionDto>> Handle(CreateSectionCommand request, CancellationToken cancellationToken)
        => moduleAccessService.CreateSectionAsync(request.PageId, request.Name, request.Key, request.SortOrder, cancellationToken);
}
