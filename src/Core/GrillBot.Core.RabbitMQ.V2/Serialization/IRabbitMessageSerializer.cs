using System.Text;

namespace GrillBot.Core.RabbitMQ.V2.Serialization;

public interface IRabbitMessageSerializer
{
    Task<byte[]> SerializeMessageAsync<T>(T data, Encoding? encoding = null, CancellationToken cancellationToken = default);
    Task<string> DeserializeToStringAsync(byte[] bytes, Encoding? encoding = null, CancellationToken cancellationToken = default);
}
