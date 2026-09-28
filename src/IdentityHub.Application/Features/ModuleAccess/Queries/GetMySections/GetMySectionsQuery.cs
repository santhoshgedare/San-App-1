using IdentityHub.Application.Common.Interfaces;
using MediatR;

namespace IdentityHub.Application.Features.ModuleAccess.Queries.GetMySections;

/// <summary>
/// Returns the section keys accessible to the current user (union across their roles),
/// consumed by the Angular <c>canRender</c> directive to decide what to render.
/// SuperAdmins (Admin role) implicitly get every section.
/// </summary>
public sealed record GetMySectionsQuery(IReadOnlyCollection<string> RoleNames, bool IsAdmin) : IRequest<IReadOnlyList<string>>;

public sealed class GetMySectionsQueryHandler(IModuleAccessService moduleAccessService)
    : IRequestHandler<GetMySectionsQuery, IReadOnlyList<string>>
{
    public async Task<IReadOnlyList<string>> Handle(GetMySectionsQuery request, CancellationToken cancellationToken)
    {
        if (request.IsAdmin)
        {
            var all = await moduleAccessService.GetModuleTreeAsync(cancellationToken);
            return all.SelectMany(m => m.Pages).SelectMany(p => p.Sections).Select(s => s.Key).Distinct().ToList();
        }

        return await moduleAccessService.GetSectionKeysForRolesAsync(request.RoleNames, cancellationToken);
    }
}
