using EmoteService.Core.Entity;
using GrillBot.Contracts.Emote.Events.Suggestions;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Core.Managers.Performance;
using GrillBot.Services.Common.Infrastructure.Api;
using Wolverine;

namespace EmoteService.Actions.EmoteSuggestions;

public class SetSuggestionApprovalAction(
    ICounterManager counterManager,
    EmoteServiceContext dbContext,
    ICurrentUserProvider _currentUser,
    IMessageBus _rabbitPublisher
) : ApiAction<EmoteServiceContext>(counterManager, dbContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var suggestionId = GetParameter<Guid>(0);
        var isApproved = GetParameter<bool>(1);

        if (!_currentUser.IsLogged)
            return new ApiResult(StatusCodes.Status403Forbidden, new { Message = "User is not logged. Missing Authorization token" });

        var payload = new EmoteSuggestionApprovalChangePayload(suggestionId, isApproved, _currentUser.Id.ToUlong());
        await _rabbitPublisher.PublishAsync(payload);

        return ApiResult.Ok();
    }
}
