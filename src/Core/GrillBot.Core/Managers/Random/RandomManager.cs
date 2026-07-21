namespace GrillBot.Core.Managers.Random;

public class RandomManager : IRandomManager
{
    private readonly Lock _locker = new();
    private Dictionary<string, System.Random> Generators { get; } = [];

    private System.Random GetOrCreate(string key)
    {
        using (_locker.EnterScope())
        {
            if (!Generators.ContainsKey(key))
                Generators.Add(key, new System.Random());

            return Generators[key];
        }
    }

    public int GetNext(string key, int min, int max)
        => GetOrCreate(key).Next(min, max);

    public int GetNext(string key, int max)
        => GetOrCreate(key).Next(max);
}
