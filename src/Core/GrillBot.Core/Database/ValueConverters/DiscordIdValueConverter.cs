using GrillBot.Core.Database.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GrillBot.Core.Database.ValueConverters;

public class DiscordIdValueConverter : ValueConverter<DiscordIdValueObject, decimal>
{
    public DiscordIdValueConverter() : base(
        v => v.Value,
        v => new DiscordIdValueObject(unchecked((ulong)v))
    )
    {
    }
}
