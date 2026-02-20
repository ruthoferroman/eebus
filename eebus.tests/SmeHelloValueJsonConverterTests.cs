using System.Text.Json;
using eebus.Ship;
using eebus.Ship.Serialization;
using Xunit;

public class SmeHelloValueJsonConverterTests
{
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new SmeHelloValueJsonConverter() }
    };

    [Fact]
    public void Serialize_ReadyPhase_WritesExpectedJson()
    {
        var value = new SmeHelloValue(
            new ConnectionHelloType(ConnectionHelloPhaseType.Ready, 123, true, null)
        );
        var json = JsonSerializer.Serialize(value, _options);

        Assert.Contains("\"phase\":\"ready\"", json);
        Assert.Contains("\"waiting\":123", json);
        Assert.Contains("\"prolongationRequest\":true", json);
    }

    [Fact]
    public void Deserialize_ReadyPhase_ReadsExpectedObject()
    {
        var json = """
        {
            "connectionHello": [
                { "phase": "ready" },
                { "waiting": 123 },
                { "prolongationRequest": true }
            ]
        }
        """;
        var value = JsonSerializer.Deserialize<SmeHelloValue>(json, _options);

        Assert.NotNull(value);
        Assert.Equal(ConnectionHelloPhaseType.Ready, value.ConnectionHello.Phase);
        Assert.Equal((uint)123, value.ConnectionHello.Waiting);
        Assert.True(value.ConnectionHello.ProlongationRequest);
    }

    [Fact]
    public void Serialize_AbortedPhase_WritesExpectedJson()
    {
        var value = new SmeHelloValue(
            new ConnectionHelloType(ConnectionHelloPhaseType.Aborted)
        );
        var json = JsonSerializer.Serialize(value, _options);

        Assert.Contains("\"phase\":\"aborted\"", json);
        Assert.DoesNotContain("waiting", json);
        Assert.DoesNotContain("prolongationRequest", json);
    }

    [Fact]
    public void Deserialize_AbortedPhase_ReadsExpectedObject()
    {
        var json = """
        {
            "connectionHello": [
                { "phase": "aborted" }
            ]
        }
        """;
        var value = JsonSerializer.Deserialize<SmeHelloValue>(json, _options);

        Assert.NotNull(value);
        Assert.Equal(ConnectionHelloPhaseType.Aborted, value.ConnectionHello.Phase);
        Assert.Null(value.ConnectionHello.Waiting);
        Assert.Null(value.ConnectionHello.ProlongationRequest);
    }
}