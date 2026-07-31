using GrillBot.Core.Infrastructure.Auth;
using PointsService.Core.Entity;
using PointsService.Handlers.Abstractions;
using GrillBot.Contracts.Points.Events;

namespace PointsService.Handlers.UserRecalculation;

public partial class UserRecalculationHandler(
    IServiceProvider serviceProvider
) : BasePointsEvent(serviceProvider)
{
    public async Task HandleAsync(UserRecalculationPayload message, CancellationToken cancellationToken)
    {
        var user = await FindOrCreateUserAsync(message.GuildId, message.UserId);

        await ProcessActionAsync(ComputeUserInfoAsync, user, nameof(ComputeUserInfoAsync));
        await ProcessActionAsync(ComputeDailyStatsAsync, user, nameof(ComputeDailyStatsAsync));
        await ProcessActionAsync(ComputeLeaderboardAsync, user, nameof(ComputeLeaderboardAsync));
        await ProcessActionAsync(ComputePositionAsync, user, nameof(ComputePositionAsync));
        await ProcessActionAsync(ComputeTelemetryAsync, user, nameof(ComputeTelemetryAsync));

        return;
    }

    private async Task ProcessActionAsync(Func<User, Task> action, User user, string actionName)
    {
        actionName = actionName.Replace("Async", "");

        using (CreateCounter(actionName))
            await action(user);

        await ContextHelper.SaveChangesAsync();
    }
}
