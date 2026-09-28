namespace IdentityHub.Application.Common.Interfaces;

/// <summary>
/// Provides access to the identity of the currently authenticated request, abstracted away from HttpContext.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsInRole(string role);
}
