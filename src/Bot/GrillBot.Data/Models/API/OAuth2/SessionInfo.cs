using System.Collections.Generic;

namespace GrillBot.Data.Models.API.OAuth2;

/// <summary>
/// The signed-in user of the current AdminWeb session.
/// </summary>
public class SessionInfo
{
    /// <summary>Discord user ID.</summary>
    public string Id { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public List<string> Permissions { get; set; } = [];
}
