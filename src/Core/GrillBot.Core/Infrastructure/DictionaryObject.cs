namespace GrillBot.Core.Infrastructure;

public class DictionaryObject<TKey, TValue> : Dictionary<TKey, TValue?>, IDictionaryObject where TKey : notnull
{
    public void FromCollection(IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
        foreach (var pair in items)
            Add(pair.Key, pair.Value);
    }

    public Dictionary<string, string?> ToDictionary()
        => this.ToDictionary(o => o.Key.ToString()!, o => o.Value?.ToString());
}
