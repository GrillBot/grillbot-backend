using GrillBot.Core.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace UnverifyService.Models.Request.Keepables;

public class CreateKeepableRequest : IDictionaryObject
{
    [MaxLength(100)]
    public string Group { get; set; } = "-";

    [MaxLength(100)]
    public string Name { get; set; } = null!;

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { "Group", Group },
            { "Name", Name }
        };
    }
}
