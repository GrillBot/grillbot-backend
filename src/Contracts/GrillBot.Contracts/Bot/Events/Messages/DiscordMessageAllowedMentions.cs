using Discord;

namespace GrillBot.Contracts.Bot.Events.Messages;

public class DiscordMessageAllowedMentions
{
    public AllowedMentionTypes? AllowedTypes { get; set; }
    public List<ulong> RoleIds { get; set; } = [];
    public List<ulong> UserIds { get; set; } = [];
    public bool? MentionRepliedUser { get; set; }

    public DiscordMessageAllowedMentions()
    {
    }

    public DiscordMessageAllowedMentions(AllowedMentionTypes? allowedTypes, List<ulong> roleIds, List<ulong> userIds, bool? mentionRepliedUser)
    {
        AllowedTypes = allowedTypes;
        RoleIds = roleIds;
        UserIds = userIds;
        MentionRepliedUser = mentionRepliedUser;
    }

    public AllowedMentions ToAllowedMentions()
    {
        return new AllowedMentions(AllowedTypes)
        {
            MentionRepliedUser = MentionRepliedUser,
            RoleIds = RoleIds,
            UserIds = UserIds
        };
    }
}