namespace IdentityHub.Domain.Enums;

/// <summary>Admin approval state of a seller. Only Approved and active sellers may sell.</summary>
public enum SellerStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
