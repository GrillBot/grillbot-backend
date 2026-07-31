using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using RubbergodService.Core.Entity;
using GrillBot.Contracts.Rubbergod.Events.Karma;

namespace RubbergodService.Handlers.Karma;

public class StoreKarmaEventHandler(
    IServiceProvider serviceProvider
) : EventHandlerBaseWithDb<RubbergodServiceContext>(serviceProvider)
{
    public async Task HandleAsync(KarmaBatchPayload message, CancellationToken cancellationToken)
    {
        foreach (var chunk in message.Users.Where(o => !string.IsNullOrEmpty(o.MemberId)).Chunk(50))
        {
            var entities = await GetOrCreateEntitiesAsync(chunk.Select(o => o.MemberId).ToList());

            foreach (var user in chunk)
            {
                if (!entities.TryGetValue(user.MemberId, out var entity))
                    continue;

                entity.KarmaValue = user.Karma;
                entity.Positive = user.Positive;
                entity.Negative = user.Negative;
            }
        }

        await ContextHelper.SaveChangesAsync(cancellationToken);
        return;
    }

    private async Task<Dictionary<string, Core.Entity.Karma>> GetOrCreateEntitiesAsync(List<string> memberIds)
    {
        var query = DbContext.Karma.Where(o => memberIds.Contains(o.MemberId));
        var entities = await ContextHelper.ReadEntitiesAsync(query);

        if (entities.Count == memberIds.Count)
            return entities.ToDictionary(o => o.MemberId, o => o);

        foreach (var memberId in memberIds.Where(id => !entities.Exists(e => e.MemberId == id)))
        {
            var entity = new Core.Entity.Karma { MemberId = memberId };

            entities.Add(entity);
            await DbContext.AddAsync(entity);
        }

        return entities.ToDictionary(o => o.MemberId, o => o);
    }
}
