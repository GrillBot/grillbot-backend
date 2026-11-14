namespace GrillBot.Contracts.AuditLog.Enums;

/// <summary>What happened in an identity / access audit event (<see cref="LogType.Identity"/>).</summary>
public enum IdentityEventType
{
    None = 0,

    /// <summary>The user signed in (an AdminWeb session or a legacy admin token was issued).</summary>
    SignIn = 1,

    /// <summary>The sign-in was refused; the reason says why.</summary>
    SignInRefused = 2,

    /// <summary>The user signed out of AdminWeb.</summary>
    SignOut = 3,

    /// <summary>
    /// The bot ended a live session itself because the user may no longer sign in
    /// (access revoked or account blocked), found while renewing its access token.
    /// </summary>
    SessionRevoked = 4
}
