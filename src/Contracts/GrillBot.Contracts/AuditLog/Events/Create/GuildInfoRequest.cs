using Discord;

namespace GrillBot.Contracts.AuditLog.Events.Create;

public class GuildInfoRequest
{
    public DefaultMessageNotifications DefaultMessageNotifications { get; set; }
    public string? Description { get; set; }
    public string? VanityUrl { get; set; }
    public string? BannerId { get; set; }
    public string? DiscoverySplashId { get; set; }
    public string? SplashId { get; set; }
    public string? IconId { get; set; }
    public byte[]? IconData { get; set; }
    public string? PublicUpdatesChannelId { get; set; }
    public string? RulesChannelId { get; set; }
    public string? SystemChannelId { get; set; }
    public string? AfkChannelId { get; set; }
    public int AfkTimeout { get; set; }
    public string Name { get; set; } = null!;
    public MfaLevel MfaLevel { get; set; }
    public VerificationLevel VerificationLevel { get; set; }
    public ExplicitContentFilterLevel ExplicitContentFilter { get; set; }
    public GuildFeature Features { get; set; }
    public PremiumTier PremiumTier { get; set; }
    public SystemChannelMessageDeny SystemChannelFlags { get; set; }
    public NsfwLevel NsfwLevel { get; set; }

    public GuildInfoRequest()
    {
    }

    public GuildInfoRequest(
        DefaultMessageNotifications defaultMessageNotifications,
        string? description,
        string? vanityUrl,
        string? bannerId,
        string? discoverySplashId,
        string? splashId,
        string? iconId,
        byte[]? iconData,
        string? publicUpdatesChannelId,
        string? rulesChannelId,
        string? systemChannelId,
        string? afkChannelId,
        int afkTimeout,
        string name,
        MfaLevel mfaLevel,
        VerificationLevel verificationLevel,
        ExplicitContentFilterLevel explicitContentFilter,
        GuildFeature features,
        PremiumTier premiumTier,
        SystemChannelMessageDeny systemChannelFlags,
        NsfwLevel nsfwLevel
    )
    {
        DefaultMessageNotifications = defaultMessageNotifications;
        Description = description;
        VanityUrl = vanityUrl;
        BannerId = bannerId;
        DiscoverySplashId = discoverySplashId;
        SplashId = splashId;
        IconId = iconId;
        IconData = iconData;
        PublicUpdatesChannelId = publicUpdatesChannelId;
        RulesChannelId = rulesChannelId;
        SystemChannelId = systemChannelId;
        AfkChannelId = afkChannelId;
        AfkTimeout = afkTimeout;
        Name = name;
        MfaLevel = mfaLevel;
        VerificationLevel = verificationLevel;
        ExplicitContentFilter = explicitContentFilter;
        Features = features;
        PremiumTier = premiumTier;
        SystemChannelFlags = systemChannelFlags;
        NsfwLevel = nsfwLevel;
    }
}
