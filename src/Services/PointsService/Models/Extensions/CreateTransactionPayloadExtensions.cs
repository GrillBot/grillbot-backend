using GrillBot.Contracts.Points.Events;
using PointsService.Enums;

namespace PointsService.Models.Extensions;

/// <summary>
/// IncrementType is this service's own persistence concern, not part of the published
/// contract, so the mapping from a transaction payload to it lives here rather than on
/// CreateTransactionPayload.
/// </summary>
public static class CreateTransactionPayloadExtensions
{
    public static IncrementType GetIncrementType(this CreateTransactionPayload payload)
    {
        if (payload.Reaction is not null)
            return payload.Reaction.IsBurst ? IncrementType.SuperReaction : IncrementType.Reaction;
        return IncrementType.Message;
    }
}
