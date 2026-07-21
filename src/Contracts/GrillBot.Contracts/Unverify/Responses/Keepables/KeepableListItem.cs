namespace GrillBot.Contracts.Unverify.Responses.Keepables;

public record KeepableListItem(
    string Group,
    string Name,
    DateTime CreatedAtUtc
);
