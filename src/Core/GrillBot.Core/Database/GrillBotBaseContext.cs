using GrillBot.Core.Database.ValueConverters;
using GrillBot.Core.Database.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Core.Database;

public class GrillBotBaseContext(DbContextOptions options) : DbContext(options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder
            .Properties<DiscordIdValueObject>()
            .HaveConversion<DiscordIdValueConverter>()
            .HaveColumnType("numeric");
    }
}
