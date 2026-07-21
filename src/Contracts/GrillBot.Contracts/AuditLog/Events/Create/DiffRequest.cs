namespace GrillBot.Contracts.AuditLog.Events.Create;

public class DiffRequest<TType>
{
    public TType? Before { get; set; }
    public TType? After { get; set; }

    public DiffRequest()
    {
    }

    public DiffRequest(TType? before, TType? after)
    {
        Before = before;
        After = after;
    }
}
