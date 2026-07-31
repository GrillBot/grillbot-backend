using GrillBot.Contracts.Points.Channels;
using GrillBot.Contracts.Points.Users;
using Wolverine;


namespace GrillBot.App.Managers.Points;

public class PointsManager
{
    private readonly PointsSynchronizationManager _synchronizationManager;
    private readonly PointsValidationManager _validationManager;

    private readonly IMessageBus _rabbitPublisher;

    public PointsManager(PointsSynchronizationManager synchronizationManager, PointsValidationManager validationManager, IMessageBus rabbitPublisher)
    {
        _synchronizationManager = synchronizationManager;
        _validationManager = validationManager;
        _rabbitPublisher = rabbitPublisher;
    }

    #region Validations

    public bool CanIncrementPoints(IMessage message, IUser? reactionUser = null) => _validationManager.CanIncrementPoints(message, reactionUser);
    public Task<bool> IsUserAcceptableAsync(IUser user) => _validationManager.IsUserAcceptableAsync(user);

    #endregion

    #region Synchronization

    public Task PushSynchronizationAsync(IGuild guild, params IUser[] users) => PushSynchronizationAsync(guild, users, []);
    public Task PushSynchronizationAsync(IGuild guild, IEnumerable<IUser> users, IEnumerable<IGuildChannel> channels) => _synchronizationManager.PushAsync(guild, users, channels);
    public Task PushSynchronizationAsync(IGuild guild, IEnumerable<UserSyncItem> users, IEnumerable<ChannelSyncItem> channels) => _synchronizationManager.PushAsync(guild, users, channels);
    public Task PushSynchronizationUsersAsync(IEnumerable<IUser> users) => _synchronizationManager.PushUsersAsync(users);

    #endregion

    #region Push

    public ValueTask PushPayloadAsync<TPayload>(TPayload payload) where TPayload : notnull
        => _rabbitPublisher.PublishAsync(payload);

    #endregion
}
