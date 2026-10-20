using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Contracts.AuditLog.Responses.Info.Dashboard;
using GrillBot.Core.Services.Common.Executor;
using UserManagementService;
using GrillBot.Contracts.UserManagement.Responses;

namespace GrillBot.App.Actions.Api.V1.Dashboard;

public class GetInfractionsDashboard : ApiAction
{
    private ITextsManager Texts { get; }

    private readonly DataResolveManager _dataResolveManager;
    private readonly IServiceClientExecutor<IUserManagementServiceClient> _userManagementService;

    public GetInfractionsDashboard(ApiRequestContext apiContext, ITextsManager texts, IServiceClientExecutor<IUserManagementServiceClient> userManagementService,
        DataResolveManager dataResolveManager) : base(apiContext)
    {
        Texts = texts;
        _userManagementService = userManagementService;
        _dataResolveManager = dataResolveManager;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var data = await _userManagementService.ExecuteRequestAsync((c, ctx) => c.GetInfractionsDashboardAsync(ctx.CancellationToken));
        var result = await MapAsync(data);

        return ApiResult.Ok(result);
    }

    private async Task<List<DashboardInfoRow>> MapAsync(List<InfractionDashboardRow> rows)
    {
        var result = new List<DashboardInfoRow>();

        foreach (var row in rows)
        {
            var user = await _dataResolveManager.GetUserAsync(row.UserId.ToUlong());

            result.Add(new DashboardInfoRow
            {
                Result = GetText(row.Type),
                Name = FormatUser(user, row.UserId)
            });
        }

        return result;
    }

    private string GetText(string id)
        => Texts[$"User/Infractions/{id}", ApiContext.Language];

    private string FormatUser(Data.Models.API.Users.User? user, string defaultValue)
    {
        if (user is null)
            return string.Format(GetText("UnknownUser"), defaultValue);

        var entity = new Database.Entity.User
        {
            Username = user.Username,
            GlobalAlias = user.GlobalAlias,
        };

        return $"User({user.Id}/{entity.GetDisplayName()})";
    }
}
