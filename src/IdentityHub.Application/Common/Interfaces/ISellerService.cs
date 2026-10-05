using IdentityHub.Application.Common.Models;

namespace IdentityHub.Application.Common.Interfaces;

/// <summary>Seller companies: created by admins first, a manager account can be linked later.</summary>
public interface ISellerService
{
    Task<IReadOnlyList<SellerDto>> GetAllAsync(CancellationToken ct);
    Task<SellerDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<SellerDto?> GetForUserAsync(Guid userId, CancellationToken ct);
    Task<Guid?> GetSellerIdForUserAsync(Guid userId, CancellationToken ct);
    Task<Result<SellerDto>> CreateAsync(SellerInput input, CancellationToken ct);
    Task<Result> UpdateAsync(Guid id, SellerInput input, bool allowLinkChange, CancellationToken ct);
    Task<Result<SellerInviteResultDto>> CreateInviteAsync(Guid id, string email, string clientBaseUrl, CancellationToken ct);
    Task<Result> SendInviteEmailAsync(Guid id, IReadOnlyList<string> to, IReadOnlyList<string> cc, string subject, string body, IReadOnlyList<EmailAttachment> attachments, CancellationToken ct);
    Task<SellerInviteInfoDto?> GetInviteAsync(string token, CancellationToken ct);
    Task<Result<AuthResultDto>> AcceptInviteAsync(string token, SellerRegistrationInput input, CancellationToken ct);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct);
}