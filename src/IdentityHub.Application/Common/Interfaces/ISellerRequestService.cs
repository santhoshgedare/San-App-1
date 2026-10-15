using IdentityHub.Application.Common.Models;

namespace IdentityHub.Application.Common.Interfaces;

public sealed record SellerRequestInput(
    string Name,
    string Email,
    string Phone,
    string? BusinessName,
    string? City,
    string? CraftType,
    string? Website,
    string? Message);

public sealed record SellerRequestDto(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    string? BusinessName,
    string? City,
    string? CraftType,
    string? Website,
    string? Message,
    string Status,
    string? AdminNote,
    DateTimeOffset CreatedAt);

/// <summary>Seller applications submitted from the public site and managed by admins.</summary>
public interface ISellerRequestService
{
    Task<Result> SubmitAsync(SellerRequestInput input, CancellationToken ct);
    Task<IReadOnlyList<SellerRequestDto>> GetAllAsync(CancellationToken ct);
    Task<Result> UpdateStatusAsync(Guid id, string status, string? adminNote, CancellationToken ct);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct);
}
