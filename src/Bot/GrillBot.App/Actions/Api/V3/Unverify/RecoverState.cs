using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Contracts.Bot;
using UnverifyService;
using GrillBot.Contracts.Unverify.Events;
using Wolverine;


namespace GrillBot.App.Actions.Api.V3.Unverify;

public class RecoverState(
    ApiRequestContext apiContext,
    ITextsManager _texts,
    IServiceClientExecutor<IUnverifyServiceClient> _unverifyClient,
    IMessageBus _rabbitPublisher
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var logId = GetParameter<Guid>(0);

        try
        {
            await _unverifyClient.ExecuteRequestAsync(
                async (client, ctx) => await client.CheckRecoveryRequirementsAsync(logId, cancellationToken: CancellationToken),
                CancellationToken
            );
        }
        catch (ClientBadRequestException ex)
        {
            if (string.IsNullOrEmpty(ex.RawData))
                throw;

            var validationError = JsonConvert.DeserializeObject<LocalizedMessageContent>(ex.RawData)!;
            var errorMessage = _texts[validationError, ApiContext.Language];

            throw new ValidationException(errorMessage).ToBadRequestValidation(logId, nameof(logId));
        }

        var payload = new RecoverAccessMessage(null, logId);

        await _rabbitPublisher.PublishAsync(payload);
        return ApiResult.Ok();
    }
}
