using GrillBot.Core.Infrastructure;

namespace GrillBot.Contracts.Unverify.Requests.Users;

public class ModifyUserRequest : IDictionaryObject
{
    public TimeSpan? SelfUnverifyMinimalTime { get; set; }
    public bool IsBotAdmin { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            ["selfUnverifyMinimalTime"] = SelfUnverifyMinimalTime?.ToString(),
            ["isBotAdmin"] = IsBotAdmin.ToString()
        };
    }
}
