using GrillBot.Services.Common.EntityFramework;
using GrillBot.Services.Common.Telemetry.Database;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Core.Entity;

/// <summary>
/// Holds the OpenIddict stores: registered clients, authorizations and token entries
/// (refresh tokens are reference tokens kept here, so they can be revoked).
/// </summary>
public class IdentityContext(DbContextOptions options, DatabaseTelemetryCollector collector) : GrillBotServiceDbContext(options, collector)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseOpenIddict();
    }
}
