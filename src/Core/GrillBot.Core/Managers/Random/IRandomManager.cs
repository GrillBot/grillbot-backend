namespace GrillBot.Core.Managers.Random;

public interface IRandomManager
{
    int GetNext(string key, int min, int max);
    int GetNext(string key, int max);
}
