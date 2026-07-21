using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Attributes;
using GrillBot.Contracts.UserManagement.Responses;
using Refit;

namespace UserManagementService;

[Service("UserManagementService")]
public interface IUserManagementServiceClient : IServiceClient
{
    [Get("/api/guild/{guildId}/users/with-nickname")]
    Task<Dictionary<string, string>> GetGuildUsersWithNicknameAsync(ulong guildId, CancellationToken cancellationToken = default);

    [Get("/api/user/{userId}")]
    Task<UserInfo> GetUserInfoAsync(ulong userId, CancellationToken cancellationToken = default);
}
