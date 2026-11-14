using GrillBot.Common.Models;
using GrillBot.Contracts.Identity;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.OAuth2;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GrillBot.App.Actions.Api.V3.Session;

/// <summary>Who is signed in. 204 when nobody is, so AdminWeb can ask without triggering a 401.</summary>
public class GetSessionAction(ApiRequestContext _apiContext) : ApiAction(_apiContext)
{
    public override Task<ApiResult> ProcessAsync()
    {
        var user = HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
            return Task.FromResult(new ApiResult(StatusCodes.Status204NoContent));

        var session = new SessionInfo
        {
            Id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "",
            Username = user.Identity.Name ?? "",
            AvatarUrl = user.FindFirst(ClaimTypes.UserData)?.Value,
            Permissions = [..
                (user.FindFirst(IdentityProtocol.ClaimTypes.Permissions)?.Value ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ]
        };

        return Task.FromResult(ApiResult.Ok(session));
    }
}
