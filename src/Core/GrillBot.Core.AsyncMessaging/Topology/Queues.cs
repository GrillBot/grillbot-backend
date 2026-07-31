namespace GrillBot.Core.AsyncMessaging.Topology;

/// <summary>
/// The one queue each deployable listens to. Every name here is a key in
/// docker/deployables.json, which is also the image tag, the Swarm service name
/// (grillbot_&lt;name&gt;) and the health-check path. Renaming one means renaming all four.
/// </summary>
public static class Queues
{
    public const string Bot = "bot";
    public const string AuditLogService = "audit_log_service";
    public const string EmoteService = "emote_service";
    public const string ImageProcessingService = "image_processing_service";
    public const string InviteService = "invite_service";
    public const string MessageService = "message_service";
    public const string PointsService = "points_service";
    public const string RemindService = "remind_service";
    public const string RubbergodService = "rubbergod_service";
    public const string SearchingService = "searching_service";
    public const string UnverifyService = "unverify_service";
    public const string UserManagementService = "user_management_service";
    public const string UserMeasuresService = "user_measures_service";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Bot,
        AuditLogService,
        EmoteService,
        ImageProcessingService,
        InviteService,
        MessageService,
        PointsService,
        RemindService,
        RubbergodService,
        SearchingService,
        UnverifyService,
        UserManagementService,
        UserMeasuresService
    };
}
