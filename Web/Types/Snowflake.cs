using System.Diagnostics.CodeAnalysis;

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