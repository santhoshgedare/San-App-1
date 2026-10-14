namespace IdentityHub.Domain.Enums;

public enum CustomRequestStatus
{
    Open = 0,
    Quoted = 1,
    Awarded = 2,
    Cancelled = 3
}

public enum CustomRequestThreadStatus
{
    Active = 0,
    Withdrawn = 1,
    Awarded = 2,
    Closed = 3
}

/// <summary>Production stages a seller moves a custom order through after it is confirmed.</summary>
public enum CustomOrderStage
{
    Accepted = 0,
    DesignFinalized = 1,
    InProduction = 2,
    QualityCheck = 3,
    ReadyToShip = 4
}
