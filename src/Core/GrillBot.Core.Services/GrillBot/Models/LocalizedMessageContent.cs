namespace GrillBot.Models;

public record LocalizedMessageContent(
    string Key,
    string[] Args
)
{
    public static implicit operator string(LocalizedMessageContent response) => response.Key;
    public static implicit operator LocalizedMessageContent(string key) => new(key, []);

    public virtual bool Equals(LocalizedMessageContent? other)
    {
        if (ReferenceEquals(this, other))
            return true;

        if (other is null)
            return false;

        return Key == other.Key && Args.SequenceEqual(other.Args);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Key);

        foreach (var arg in Args)
            hash.Add(arg);

        return hash.ToHashCode();
    }
}
