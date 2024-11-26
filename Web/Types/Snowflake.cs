using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Web.Types;

public readonly record struct Snowflake(long Value) : IEquatable<Snowflake>, IParsable<Snowflake>
{
    public override int GetHashCode()
        => Value.GetHashCode();

    public override string ToString()
        => Value.ToString();

    public static implicit operator Snowflake(long value) => new(value);

    public static Snowflake Parse(string s) => Parse(s, null);
    public static Snowflake Parse(string s, IFormatProvider? provider) => long.Parse(s, provider);

    public static bool TryParse(string s, out Snowflake @out) => TryParse(s, null, out @out);
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Snowflake @out)
    {
        if (long.TryParse(s, provider, out var result))
        {
            @out = new(result);
            return true;
        }

        @out = default;
        return false;
    }
}

public class SnowflakeJsonConverter : JsonConverter<Snowflake>
{
    public override Snowflake Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            if (Snowflake.TryParse(reader.GetString(), null, out var value))
                return value;

            return default;
        }
        else
            return reader.GetInt64();
    }

    public override void Write(Utf8JsonWriter writer, Snowflake value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}