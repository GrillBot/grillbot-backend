using GrillBot.Common.Services.KachnaOnline;
using GrillBot.Common.Services.Math;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GrillBot.Core.Services;
using GrillBot.Core.Services.IdentityService;

namespace GrillBot.Common.Services;

public static class ServiceExtensions
{
    public static void AddThirdPartyServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddExternalServices(configuration);
        services.AddIdentityTokenClient(configuration);

        services.RegisterService<IKachnaOnlineClient>(configuration);
        services.RegisterService<IMathClient>(configuration);
    }
}
