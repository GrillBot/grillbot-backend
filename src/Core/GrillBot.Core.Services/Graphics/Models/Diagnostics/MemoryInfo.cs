namespace Graphics.Models.Diagnostics;

public class MemoryInfo
{
    public long Rss { get; set; }
    public long HeapTotal { get; set; }
    public long HeapUsed { get; set; }
    public long External { get; set; }
    public long ArrayBuffers { get; set; }
}
