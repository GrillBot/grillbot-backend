using System.Text.Json.Serialization;

namespace GrillBot.Contracts.Rubbergod.Help;

public class Cog
{
    [JsonPropertyName("id")]
    public ulong? Id { get; set; }

    [JsonPropertyName("children")]
    public List<string> Children { get; set; } = [];
}
