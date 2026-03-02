using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eebus.Spine;

internal class DatagramJsonConverter : JsonConverter<DatagramType>
{
    public override DatagramType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of JSON object.");

        // move to first property
        if (!reader.Read())
            throw new JsonException("Unexpected end of JSON.");

        if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "datagram")
            throw new JsonException("Expected 'datagram' property.");

        // datagram -> array
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array for 'datagram'.");

        HeaderType? header = null;
        PayloadType? payload = null;

        // iterate datagram array elements (expected objects with 'header' or 'payload')
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object in 'datagram' array.");

            // read object properties
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Expected property name in datagram element.");

                var propName = reader.GetString()!;
                if (!reader.Read())
                    throw new JsonException("Unexpected end of JSON while reading datagram element value.");

                switch (propName)
                {
                    case "header":
                        header = ReadHeader(ref reader);
                        break;

                    case "payload":
                        payload = ReadPayload(ref reader);
                        break;

                    default:
                        throw new JsonException($"Unexpected property '{propName}' in datagram element.");
                }
            }
        }

        // consume trailing EndObject of the wrapper
        if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            throw new JsonException("Expected end of root JSON object.");

        if (header == null)
            throw new JsonException("Missing required 'header' element in datagram.");

        if (payload == null)
            payload = new PayloadType(null);

        return new DatagramType(header, payload);
    }

    public override void Write(Utf8JsonWriter writer, DatagramType value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("datagram");
        writer.WriteStartArray();

        // header element
        writer.WriteStartObject();
        writer.WritePropertyName("header");
        writer.WriteStartArray();

        if (!string.IsNullOrEmpty(value.Header.SpecificationVersion))
        {
            writer.WriteStartObject();
            writer.WriteString("specificationVersion", value.Header.SpecificationVersion);
            writer.WriteEndObject();
        }

        void WriteFeatureAddress(string propName, FeatureAddressType? fa)
        {
            if (fa == null) return;
            writer.WriteStartObject();
            writer.WritePropertyName(propName);
            writer.WriteStartArray();
            if (!string.IsNullOrEmpty(fa.Device))
            {
                writer.WriteStartObject();
                writer.WriteString("device", fa.Device);
                writer.WriteEndObject();
            }
            if (fa.Entity != null)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("entity");
                writer.WriteStartArray();
                foreach (var e in fa.Entity)
                    writer.WriteNumberValue(e);
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            if (fa.Feature.HasValue)
            {
                writer.WriteStartObject();
                writer.WriteNumber("feature", fa.Feature.Value);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        WriteFeatureAddress("addressSource", value.Header.AddressSource);
        WriteFeatureAddress("addressDestination", value.Header.AddressDestination);
        WriteFeatureAddress("addressOriginator", value.Header.AddressOriginator);

        if (value.Header.MsgCounter.HasValue)
        {
            writer.WriteStartObject();
            writer.WriteNumber("msgCounter", value.Header.MsgCounter.Value);
            writer.WriteEndObject();
        }

        if (value.Header.MsgCounterReference.HasValue)
        {
            writer.WriteStartObject();
            writer.WriteNumber("msgCounterReference", value.Header.MsgCounterReference.Value);
            writer.WriteEndObject();
        }

        if (!string.IsNullOrEmpty(value.Header.CmdClassifier))
        {
            writer.WriteStartObject();
            writer.WriteString("cmdClassifier", value.Header.CmdClassifier);
            writer.WriteEndObject();
        }

        if (value.Header.AckRequest.HasValue)
        {
            writer.WriteStartObject();
            writer.WriteBoolean("ackRequest", value.Header.AckRequest.Value);
            writer.WriteEndObject();
        }

        if (value.Header.Timestamp.HasValue)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("timestamp");
            value.Header.Timestamp.Value.WriteTo(writer);
            writer.WriteEndObject();
        }

        writer.WriteEndArray(); // end header array
        writer.WriteEndObject(); // end header object

        // payload element
        writer.WriteStartObject();
        writer.WritePropertyName("payload");
        writer.WriteStartArray();

        // write single payload object containing cmd array
        writer.WriteStartObject();
        writer.WritePropertyName("cmd");
        writer.WriteStartArray();
        if (value.Payload?.Cmd != null)
        {
            foreach (var cmd in value.Payload.Cmd)
            {
                JsonSerializer.Serialize(writer, cmd, options); // validate cmd object and write it
                //cmd.WriteTo(writer);
            }
        }
        writer.WriteEndArray(); // end cmd array
        writer.WriteEndObject(); // end payload object

        writer.WriteEndArray(); // end payload array
        writer.WriteEndObject(); // end payload element

        writer.WriteEndArray(); // end datagram array
        writer.WriteEndObject(); // end root
    }

    private static HeaderType ReadHeader(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array for 'header'.");

        string? specificationVersion = null;
        FeatureAddressType? addressSource = null;
        FeatureAddressType? addressDestination = null;
        FeatureAddressType? addressOriginator = null;
        uint? msgCounter = null;
        uint? msgCounterReference = null;
        string? cmdClassifier = null;
        bool? ackRequest = null;
        JsonElement? timestamp = null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object in 'header' array.");

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Expected property name in header object.");

                var prop = reader.GetString()!;
                if (!reader.Read())
                    throw new JsonException("Unexpected end of JSON while reading header property value.");

                switch (prop)
                {
                    case "specificationVersion":
                        specificationVersion = reader.GetString();
                        break;

                    case "addressSource":
                        addressSource = ReadFeatureAddress(ref reader);
                        break;

                    case "addressDestination":
                        addressDestination = ReadFeatureAddress(ref reader);
                        break;

                    case "addressOriginator":
                        addressOriginator = ReadFeatureAddress(ref reader);
                        break;

                    case "msgCounter":
                        msgCounter = reader.GetUInt32();
                        break;

                    case "msgCounterReference":
                        msgCounterReference = reader.GetUInt32();
                        break;

                    case "cmdClassifier":
                        cmdClassifier = reader.GetString();
                        break;

                    case "ackRequest":
                        ackRequest = reader.GetBoolean();
                        break;

                    case "timestamp":
                        using (var doc = JsonDocument.ParseValue(ref reader))
                            timestamp = doc.RootElement.Clone();
                        break;

                    default:
                        throw new JsonException($"Unexpected header property '{prop}'.");
                }
            }
        }

        return new HeaderType(specificationVersion, addressSource, addressDestination, addressOriginator, msgCounter, msgCounterReference, cmdClassifier, ackRequest, timestamp);
    }

    private static FeatureAddressType ReadFeatureAddress(ref Utf8JsonReader reader)
    {
        // FeatureAddress may be encoded as array of single-property objects or as an object.
        string? device = null;
        List<int>? entityList = null;
        int? feature = null;

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Expected start of object in feature address array.");

                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName)
                        throw new JsonException("Expected property name in feature address object.");

                    var prop = reader.GetString()!;
                    if (!reader.Read())
                        throw new JsonException("Unexpected end of JSON in feature address.");

                    switch (prop)
                    {
                        case "device":
                            device = reader.GetString();
                            break;
                        case "entity":
                            if (reader.TokenType != JsonTokenType.StartArray)
                                throw new JsonException("Expected array for 'entity'.");
                            entityList ??= new List<int>();
                            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                            {
                                if (reader.TokenType != JsonTokenType.Number)
                                    throw new JsonException("Expected numeric entity value.");
                                entityList.Add(reader.GetInt32());
                            }
                            break;
                        case "feature":
                            feature = reader.GetInt32();
                            break;
                        default:
                            throw new JsonException($"Unexpected feature address property '{prop}'.");
                    }
                }
            }
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Expected property name in feature address object.");

                var prop = reader.GetString()!;
                if (!reader.Read())
                    throw new JsonException("Unexpected end of JSON in feature address.");

                switch (prop)
                {
                    case "device":
                        device = reader.GetString();
                        break;
                    case "entity":
                        if (reader.TokenType != JsonTokenType.StartArray)
                            throw new JsonException("Expected array for 'entity'.");
                        entityList ??= new List<int>();
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            if (reader.TokenType != JsonTokenType.Number)
                                throw new JsonException("Expected numeric entity value.");
                            entityList.Add(reader.GetInt32());
                        }
                        break;
                    case "feature":
                        feature = reader.GetInt32();
                        break;
                    default:
                        throw new JsonException($"Unexpected feature address property '{prop}'.");
                }
            }
        }
        else
        {
            throw new JsonException("Expected start of array or object for feature address.");
        }

        return new FeatureAddressType(device, entityList?.ToArray(), feature);
    }

    private static PayloadType ReadPayload(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array for 'payload'.");

        var cmdList = new List<CmdType>();

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object in 'payload' array.");

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Expected property name in payload object.");

                var prop = reader.GetString()!;
                if (!reader.Read())
                    throw new JsonException("Unexpected end of JSON while reading payload.");

                switch (prop)
                {
                    case "cmd":
                        if (reader.TokenType != JsonTokenType.StartArray)
                            throw new JsonException("Expected start of array for 'cmd'.");

                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            reader.Read();
                            var cmd = JsonSerializer.Deserialize<CmdType>(ref reader); // validate cmd object
                            //using var doc = JsonDocument.ParseValue(ref reader);
                            cmdList.Add(cmd);
                        }
                        reader.Read();
                        break;

                    default:
                        throw new JsonException($"Unexpected payload property '{prop}'.");
                }
            }
        }

        return new PayloadType(cmdList.Count > 0 ? cmdList.ToArray() : null);
    }
}