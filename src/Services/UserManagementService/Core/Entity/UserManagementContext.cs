using GrillBot.Services.Common.EntityFramework;
using GrillBot.Services.Common.Telemetry.Database;
using Microsoft.EntityFrameworkCore;
using UserManagementService.Core.Entity.Infractions;

namespace UserManagementService.Core.Entity;

public class UserManagementContext(DbContextOptions options, DatabaseTelemetryCollector collector) : GrillBotServiceDbContext(options, collector)
{
    private const string InfractionsSchema = "infractions";

    public DbSet<GuildUserNickname> Nicknames => Set<GuildUserNickname>();
    public DbSet<GuildUser> GuildUsers => Set<GuildUser>();

    public DbSet<WarningInfraction> Warnings => Set<WarningInfraction>();
    public DbSet<TimeoutInfraction> Timeouts => Set<TimeoutInfraction>();
    public DbSet<UnverifyInfraction> Unverifies => Set<UnverifyInfraction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GuildUser>(builder =>
            builder.HasMany(o => o.Nicknames).WithOne(o => o.GuildUser).HasForeignKey(o => new { o.GuildId, o.UserId })
        );

        // Infractions live in their own schema, so the user records and the infraction history stay apart in one database.
        modelBuilder.Entity<WarningInfraction>(builder =>
        {
            builder.ToTable("Warnings", InfractionsSchema);
            builder.HasIndex(o => new { o.GuildId, o.UserId });
        });

        modelBuilder.Entity<TimeoutInfraction>(builder =>
        {
            builder.ToTable("Timeouts", InfractionsSchema);
            builder.HasIndex(o => new { o.GuildId, o.UserId });
        });

        modelBuilder.Entity<UnverifyInfraction>(builder =>
        {
            builder.ToTable("Unverifies", InfractionsSchema);
            builder.HasIndex(o => new { o.GuildId, o.UserId });
        });
    }
}
