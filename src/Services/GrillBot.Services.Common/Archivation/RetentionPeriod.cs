namespace GrillBot.Services.Common.Archivation;

/// <summary>
/// How long a record is kept. Months are calendar months, so "6 months" means the same day six months ago.
/// </summary>
public readonly record struct RetentionPeriod
{
    public int Months { get; }
    public TimeSpan Duration { get; }

    private RetentionPeriod(int months, TimeSpan duration)
    {
        Months = months;
        Duration = duration;
    }

    public static RetentionPeriod FromMonths(int months)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(months);
        return new RetentionPeriod(months, TimeSpan.Zero);
    }

    public static RetentionPeriod FromTimeSpan(TimeSpan duration)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(duration, TimeSpan.Zero);
        return new RetentionPeriod(0, duration);
    }

    /// <summary>Records created at or before the returned moment are expired.</summary>
    public DateTime GetCutoff(DateTime nowUtc)
        => Months > 0 ? nowUtc.AddMonths(-Months) : nowUtc.Subtract(Duration);

    public override string ToString()
        => Months > 0 ? $"{Months} months" : Duration.ToString();
}
