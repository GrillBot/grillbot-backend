namespace GrillBot.Core.Infrastructure;

public interface IDictionaryObject
{
    Dictionary<string, string?> ToDictionary();
}
