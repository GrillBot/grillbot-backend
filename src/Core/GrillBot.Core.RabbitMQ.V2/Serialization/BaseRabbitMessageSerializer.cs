using System.Text;

namespace GrillBot.Core.RabbitMQ.V2.Serialization;

public class BaseRabbitMessageSerializer : IRabbitMessageSerializer
{
    public Task<string> DeserializeToStringAsync(byte[] bytes, Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        var message = (encoding ?? Encoding.UTF8).GetString(bytes);
        return Task.FromResult(message);
    }

    public virtual Task<byte[]> SerializeMessageAsync<T>(T data, Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        var bytes = (encoding ?? Encoding.UTF8).GetBytes(data?.ToString() ?? "");
        return Task.FromResult(bytes);
    }
}
