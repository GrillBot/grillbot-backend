using Discord;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Core.Database.ValueObjects;

public readonly struct DiscordIdValueObject(ulong value) : IEquatable<DiscordIdValueObject>, IComparable<DiscordIdValueObject>
{
    public ulong Value => value;

    public static implicit operator ulong(DiscordIdValueObject id) => id.Value;
    public static implicit operator DiscordIdValueObject(ulong value) => new(value);

    // Snowflake utils
    public static implicit operator DiscordIdValueObject(DateTimeOffset stamp) => new(SnowflakeUtils.ToSnowflake(stamp));
    public static implicit operator DateTimeOffset(DiscordIdValueObject id) => SnowflakeUtils.FromSnowflake(id.Value);

    public override string ToString()
    {
        return Value.ToString();
    }

    public int CompareTo(DiscordIdValueObject other) => Value.CompareTo(other.Value);

    public bool Equals(DiscordIdValueObject other) => Value == other.Value;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is DiscordIdValueObject other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(DiscordIdValueObject left, DiscordIdValueObject right) => left.Equals(right);
    public static bool operator !=(DiscordIdValueObject left, DiscordIdValueObject right) => !left.Equals(right);
    public static bool operator <(DiscordIdValueObject left, DiscordIdValueObject right) => left.Value < right.Value;
    public static bool operator <=(DiscordIdValueObject left, DiscordIdValueObject right) => left.Value <= right.Value;
    public static bool operator >(DiscordIdValueObject left, DiscordIdValueObject right) => left.Value > right.Value;
    public static bool operator >=(DiscordIdValueObject left, DiscordIdValueObject right) => left.Value >= right.Value;
}
