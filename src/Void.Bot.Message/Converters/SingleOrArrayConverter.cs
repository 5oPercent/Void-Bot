using System.Text.Json;
using System.Text.Json.Serialization;

namespace Void.Bot.Message.Converters;

public class SingleOrArrayConverter<T> : JsonConverter<List<T>>
{
    public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        List<T> result = new();

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                result.Add(JsonSerializer.Deserialize<T>(ref reader, options));
            }
        }
        else
        {
            result.Add(JsonSerializer.Deserialize<T>(ref reader, options));
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}