using System.Text.Json;

namespace eebus.Ship.Serialization;

internal class AccessMethodsJsonConverter : JsonConverter<AccessMethodsType>
{
    public override AccessMethodsType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of JSON object.");

        string? id = null;
        AccessMethodsDnsSdMdnsType? dnsSd = null;
        AccessMethodsDnsType? dns = null;

        // move to first property
        if (!reader.Read())
            throw new JsonException("Unexpected end of JSON.");

        if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "accessMethods")
            throw new JsonException("Expected 'accessMethods' property.");

        // move to value (should be StartArray)
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array for 'accessMethods'.");

        // read array elements
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object in 'accessMethods' array.");

            // read object properties
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Expected property name in accessMethods object.");

                string propName = reader.GetString();
                // move to property value
                if (!reader.Read())
                    throw new JsonException("Unexpected end of JSON while reading property value.");

                switch (propName)
                {
                    case "id":
                        if (reader.TokenType != JsonTokenType.String)
                            throw new JsonException("Expected string value for 'id'.");
                        id = reader.GetString();
                        break;

                    case "dnsSd_mDns":
                        // tolerant: dnsSd_mDns expected as an array of objects (empty complex type)
                        if (reader.TokenType == JsonTokenType.StartArray)
                        {
                            // consume array contents (we only care that it exists)
                            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                            {
                                if (reader.TokenType == JsonTokenType.StartObject)
                                {
                                    // skip object contents until EndObject
                                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject) { }
                                }
                            }
                            dnsSd = new AccessMethodsDnsSdMdnsType();
                        }
                        else if (reader.TokenType == JsonTokenType.StartObject)
                        {
                            // tolerate direct object
                            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject) { }
                            dnsSd = new AccessMethodsDnsSdMdnsType();
                        }
                        else if (reader.TokenType == JsonTokenType.Null)
                        {
                            dnsSd = null;
                        }
                        else
                        {
                            throw new JsonException("Unexpected token for 'dnsSd_mDns'.");
                        }
                        break;

                    case "dns":
                        // dns is expected as array containing an object with "uri"
                        if (reader.TokenType != JsonTokenType.StartArray)
                            throw new JsonException("Expected start of array for 'dns'.");

                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            if (reader.TokenType != JsonTokenType.StartObject)
                                throw new JsonException("Expected start of object in 'dns' array.");

                            string? uri = null;
                            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                            {
                                if (reader.TokenType != JsonTokenType.PropertyName)
                                    throw new JsonException("Expected property name in dns object.");

                                var dnsProp = reader.GetString();
                                if (!reader.Read())
                                    throw new JsonException("Unexpected end of JSON in dns object.");

                                if (dnsProp == "uri")
                                {
                                    if (reader.TokenType != JsonTokenType.String)
                                        throw new JsonException("Expected string for 'uri'.");
                                    uri = reader.GetString();
                                }
                                else
                                {
                                    throw new JsonException($"Unexpected property '{dnsProp}' in dns object.");
                                }
                            }

                            if (uri != null)
                                dns = new AccessMethodsDnsType(uri);
                        }
                        break;

                    default:
                        throw new JsonException($"Unexpected property '{propName}' in accessMethods object.");
                }
            }
        }

        // move past EndObject of root
        if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            throw new JsonException("Expected end of root JSON object.");

        if (id == null)
            throw new JsonException("Missing required 'id' property in accessMethods.");

        return new AccessMethodsType(id, dnsSd, dns);
    }

    public override void Write(Utf8JsonWriter writer, AccessMethodsType value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("accessMethods");
        writer.WriteStartArray();

        // id
        writer.WriteStartObject();
        writer.WriteString("id", value.Id);
        writer.WriteEndObject();

        // dnsSd_mDns (empty complex type) - write only if present
        if (value.DnsSd_mDns is not null)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("dnsSd_mDns");
            writer.WriteStartArray();
            writer.WriteStartObject();
            writer.WriteEndObject();
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        // dns
        if (value.Dns is not null)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("dns");
            writer.WriteStartArray();
            writer.WriteStartObject();
            writer.WriteString("uri", value.Dns.Uri);
            writer.WriteEndObject();
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}