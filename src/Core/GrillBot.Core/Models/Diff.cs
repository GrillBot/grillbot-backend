namespace GrillBot.Core.Models;

public class Diff<TType>
{
    public TType Before { get; set; }
    public TType After { get; set; }

    public Diff()
    {
        Before = default!;
        After = default!;
    }

    public Diff(TType before, TType after)
    {
        Before = before;
        After = after;
    }

    public bool IsEmpty()
        => Comparer<TType>.Default.Compare(Before, After) == 0;
}
