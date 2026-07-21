using System.Text.Json.Serialization;

namespace RubbergodService.Models.Help;

public class Cog
{
    [JsonPropertyName("id")]
    public ulong? Id { get; set; }

    [JsonPropertyName("children")]
    public List<string> Children { get; set; } = [];
}
