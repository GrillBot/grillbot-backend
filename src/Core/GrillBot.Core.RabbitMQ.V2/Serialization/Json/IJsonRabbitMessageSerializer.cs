using System.Text;
using System.Text.Json.Nodes;

namespace GrillBot.Core.RabbitMQ.V2.Serialization.Json;

public interface IJsonRabbitMessageSerializer : IRabbitMessageSerializer
{
    Task<JsonNode?> DeserializeToJsonObjectAsync(byte[] bytes, Encoding? encoding = null, CancellationToken cancellationToken = default);
}
