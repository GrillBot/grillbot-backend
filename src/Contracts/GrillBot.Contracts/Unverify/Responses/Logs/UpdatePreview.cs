namespace GrillBot.Contracts.Unverify.Responses.Logs;

public record UpdatePreview(
    DateTime NewStartAtUtc,
    DateTime NewEndAtUtc,
    string? Reason = null
);
