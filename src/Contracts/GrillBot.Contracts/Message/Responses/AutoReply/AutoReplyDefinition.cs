namespace GrillBot.Contracts.Message.Responses.AutoReply;

public record AutoReplyDefinition(
    Guid Id,
    string Template,
    string Reply,
    bool IsDeleted,
    bool IsDisabled,
    bool IsCaseSensitive
);
