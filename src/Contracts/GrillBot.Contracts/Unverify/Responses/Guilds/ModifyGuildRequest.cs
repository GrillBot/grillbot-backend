using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Contracts.Unverify.Responses.Guilds;

public class ModifyGuildRequest
{
    [DiscordId]
    [StringLength(32)]
    public string? MuteRoleId { get; set; }
}
