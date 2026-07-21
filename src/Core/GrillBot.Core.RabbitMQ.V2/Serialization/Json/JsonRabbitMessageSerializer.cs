using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GrillBot.Core.RabbitMQ.V2.Serialization.Json;

internal class JsonRabbitMessageSerializer : BaseRabbitMessageSerializer, IJsonRabbitMessageSerializer
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        IgnoreReadOnlyProperties = true
    };

    public async Task<JsonNode?> DeserializeToJsonObjectAsync(byte[] bytes, Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        var rawMessage = await DeserializeToStringAsync(bytes, encoding, cancellationToken);
        return JsonSerializer.Deserialize<JsonNode>(rawMessage, SerializerOptions);
    }

    public override Task<byte[]> SerializeMessageAsync<T>(T data, Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(data, SerializerOptions);
        var bytes = (encoding ?? Encoding.UTF8).GetBytes(json);

        return Task.FromResult(bytes);
    }
}
