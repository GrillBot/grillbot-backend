using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using GrillBot.Core.RabbitMQ.V2.Serialization;
using GrillBot.Core.RabbitMQ.V2.Serialization.Json;

#pragma warning disable S1144
namespace GrillBot.Core.RabbitMQ.V2.Tests.Serialization.Json;

[TestClass]
public class JsonRabbitMessageSerializerTests
{
    private readonly JsonRabbitMessageSerializer _serializer = new();

    public TestContext TestContext { get; set; }

    private class TestClass
    {
        public string NormalProperty { get; set; } = "A";
        public int Number { get; set; } = 42;
        public string? NullProperty { get; set; }
        public string ReadWrite { get; set; } = "RW";
        public static string ReadOnly => "RO";
    }

    [TestMethod]
    public async Task SerializeMessageAsync_SerializesObjectToJsonBytes_CamelCase()
    {
        var obj = new TestClass();
        var bytes = await _serializer.SerializeMessageAsync(obj, cancellationToken: CancellationToken.None);

        var json = Encoding.UTF8.GetString(bytes);
        var node = JsonNode.Parse(json);

        Assert.IsNotNull(node);
        // CamelCase property names
        Assert.IsNotNull(node["normalProperty"]);
        Assert.IsNotNull(node["number"]);
        Assert.IsNotNull(node["readWrite"]);
        // Should not contain PascalCase
        Assert.IsNull(node["NormalProperty"]);
        Assert.IsNull(node["ReadWrite"]);
        // Should not contain read-only property
        Assert.IsNull(node["readOnly"]);
    }

    [TestMethod]
    public async Task SerializeMessageAsync_DoesNotWriteIndented()
    {
        var obj = new TestClass();
        var bytes = await _serializer.SerializeMessageAsync(obj, cancellationToken: TestContext.CancellationToken);
        var json = Encoding.UTF8.GetString(bytes);

        // Should not contain newlines or indentation
        Assert.IsFalse(json.Contains('\n') || json.Contains('\r') || json.Contains("  "));
    }

    [TestMethod]
    public async Task DeserializeToJsonObjectAsync_DeserializesJsonBytesToJsonNode()
    {
        const string json = "{\"foo\":42}";
        var bytes = Encoding.UTF8.GetBytes(json);

        var node = await _serializer.DeserializeToJsonObjectAsync(bytes, cancellationToken: TestContext.CancellationToken);

        Assert.IsNotNull(node);
        Assert.IsNotNull(node["foo"]);
        Assert.AreEqual("42", node["foo"]!.ToString());
    }

    [TestMethod]
    public async Task DeserializeToJsonObjectAsync_WithEncoding_UsesProvidedEncoding()
    {
        const string json = "{\"bar\":\"baz\"}";
        var encoding = Encoding.Unicode;
        var bytes = encoding.GetBytes(json);

        var node = await _serializer.DeserializeToJsonObjectAsync(bytes, encoding, TestContext.CancellationToken);

        Assert.IsNotNull(node);
        Assert.IsNotNull(node["bar"]);
        Assert.AreEqual("baz", node["bar"]!.ToString());
    }

    [TestMethod]
    public async Task DeserializeToJsonObjectAsync_PropertyNameCaseInsensitive()
    {
        const string json = "{\"NORMALPROPERTY\":\"abc\",\"number\":123}";
        var bytes = Encoding.UTF8.GetBytes(json);

        var node = await _serializer.DeserializeToJsonObjectAsync(bytes, cancellationToken: TestContext.CancellationToken);

        // Should be able to access property regardless of case
        Assert.AreEqual("abc", node?["normalProperty"]?.ToString());
        Assert.AreEqual("abc", node?["NORMALPROPERTY"]?.ToString());
        Assert.AreEqual("123", node?["number"]?.ToString());
    }

    [TestMethod]
    public async Task SerializeMessageAsync_IgnoreReadOnlyProperties()
    {
        var obj = new TestClass();
        var bytes = await _serializer.SerializeMessageAsync(obj, cancellationToken: TestContext.CancellationToken);
        var json = Encoding.UTF8.GetString(bytes);

        // ReadOnly property should not be present
        Assert.DoesNotContain("readOnly", json);
    }

    [TestMethod]
    public async Task DeserializeToJsonObjectAsync_InvalidJson_ThrowsJsonException()
    {
        const string invalidJson = "not a json";
        var bytes = Encoding.UTF8.GetBytes(invalidJson);

        await Assert.ThrowsExactlyAsync<JsonException>(() => _serializer.DeserializeToJsonObjectAsync(bytes, cancellationToken: TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task Base_DeserializeToStringAsync_ReturnsString()
    {
        const string str = "hello";
        var bytes = Encoding.UTF8.GetBytes(str);

        // Use as base class
        var baseSerializer = (BaseRabbitMessageSerializer)_serializer;
        var result = await baseSerializer.DeserializeToStringAsync(bytes, cancellationToken: TestContext.CancellationToken);

        Assert.AreEqual(str, result);
    }

    [TestMethod]
    public async Task Base_SerializeMessageAsync_UsesToString()
    {
        const int obj = 12345;
        var baseSerializer = new BaseRabbitMessageSerializer();
        var bytes = await baseSerializer.SerializeMessageAsync(obj, cancellationToken: TestContext.CancellationToken);

        var result = Encoding.UTF8.GetString(bytes);
        Assert.AreEqual("12345", result);
    }

    [TestMethod]
    public async Task Base_SerializeMessageAsync_Null_ReturnsEmpty()
    {
        var baseSerializer = new BaseRabbitMessageSerializer();
        var bytes = await baseSerializer.SerializeMessageAsync<object>(null!, cancellationToken: TestContext.CancellationToken);

        var result = Encoding.UTF8.GetString(bytes);
        Assert.AreEqual(string.Empty, result);
    }
}
