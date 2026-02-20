using System.Text.Json;
using System.Text.Json;
using Xunit;
using eebus.Ship;
using eebus.Ship.Serialization;

namespace eebus.Tests.Ship.Serialization;

public class SmeProtocolHandshakeValueJsonConverterTests
{
    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new SmeProtocolHandshakeValueJsonConverter());
        return options;
    }

    [Fact]
    public void Roundtrip_SerializeThenDeserialize_PreservesValues()
    {
        var original = new SmeProtocolHandshakeValue(
            new MessageProtocolHandshakeType(
                ProtocolHandshakeTypeType.AnnounceMax,
                new MessageProtocolHandshakeTypeVersion(1, 2),
                ["JSON-UTF8"]
            )
        );

        var options = CreateOptions();

        // Serialize using the custom converter
        var json = JsonSerializer.Serialize(original, options);

        // Deserialize back
        var result = JsonSerializer.Deserialize<SmeProtocolHandshakeValue>(json, options);

        Assert.NotNull(result);
        Assert.Equal(original.MessageProtocolHandshake.HandshakeType, result!.MessageProtocolHandshake.HandshakeType);
        Assert.Equal(original.MessageProtocolHandshake.Version.Major, result.MessageProtocolHandshake.Version.Major);
        Assert.Equal(original.MessageProtocolHandshake.Version.Minor, result.MessageProtocolHandshake.Version.Minor);
        Assert.Equal(original.MessageProtocolHandshake.Formats, result.MessageProtocolHandshake.Formats);
    }

    [Fact]
    public void Read_ParsesExpectedJsonShape()
    {
        // JSON that matches the shape written by SmeProtocolHandshakeValueJsonConverter.Write(...)
        var json = """
        {
          "messageProtocolHandshake": [
            { "handshakeType": "announceMax" },
            { "version": [ { "major": 3 }, { "minor": 4 } ] },
            { "formats": [ { "format": "alpha" }, { "format": "beta" } ] }
          ]
        }
        """;

        var options = CreateOptions();

        var result = JsonSerializer.Deserialize<SmeProtocolHandshakeValue>(json, options);

        Assert.NotNull(result);
        var msg = result!.MessageProtocolHandshake;
        Assert.Equal(ProtocolHandshakeTypeType.AnnounceMax, msg.HandshakeType);
        Assert.Equal((ushort)3, msg.Version.Major);
        Assert.Equal((ushort)4, msg.Version.Minor);
        Assert.Equal(new[] { "alpha", "beta" }, msg.Formats);
    }

    [Fact]
    public void Read_MissingRequiredFields_ThrowsJsonException()
    {
        // Missing version
        var json = """
        {
          "messageProtocolHandshake": [
            { "handshakeType": "select" },
            { "formats": [ { "format": "only" } ] }
          ]
        }
        """;

        var options = CreateOptions();

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<SmeProtocolHandshakeValue>(json, options));
    }
}