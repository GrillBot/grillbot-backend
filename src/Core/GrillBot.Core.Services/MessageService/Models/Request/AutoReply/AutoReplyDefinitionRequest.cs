using GrillBot.Core.Infrastructure;

namespace MessageService.Models.Request.AutoReply;

public class AutoReplyDefinitionRequest : IDictionaryObject
{
    public string Template { get; set; } = null!;
    public string Reply { get; set; } = null!;
    public bool IsDisabled { get; set; }
    public bool IsCaseSensitive { get; set; } = false;

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(Template), Template },
            { nameof(Reply), Reply },
            { nameof(IsDisabled), IsDisabled.ToString() },
            { nameof(IsCaseSensitive), IsCaseSensitive.ToString() }
        };
    }
}
