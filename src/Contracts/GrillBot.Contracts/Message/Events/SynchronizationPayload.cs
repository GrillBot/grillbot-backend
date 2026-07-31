using GrillBot.Contracts.Message.Events.Channels;

namespace GrillBot.Contracts.Message.Events;

public sealed record SynchronizationPayload(List<ChannelSynchronizationItem> Channels);
