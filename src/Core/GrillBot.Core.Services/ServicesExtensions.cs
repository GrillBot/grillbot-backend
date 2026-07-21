using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Attributes;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.Common.Formatters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Refit;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace GrillBot.Core.Services;

public static class ServicesExtensions
{
    public static void RegisterService<TInterface>(this IServiceCollection services, IConfiguration configuration)
        where TInterface : class, IServiceClient
    {
        var serviceName = typeof(TInterface).GetCustomAttribute<ServiceAttribute>()?.ServiceName ??
            throw new ArgumentException("Missing service attribute in the client interface.");

        var isThirdParty = configuration.GetValue<bool>($"Services:{serviceName}:IsThirdParty");
        var uri = configuration[$"Services:{serviceName}:Api"]!;

        if (string.IsNullOrEmpty(uri))
            return;

        services
            .AddScoped<IServiceClientExecutor<TInterface>, ServiceClientExecutor<TInterface>>()
            .AddRefitClient<TInterface>(new RefitSettings
            {
                ExceptionFactory = async response =>
                {
                    if (response.IsSuccessStatusCode)
                        return null;

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var url = response.RequestMessage?.RequestUri?.ToString() ?? "unknown_url";
                    var method = response.RequestMessage?.Method?.Method ?? "unknown_method";
                    var responseData = $"{method} {url}\n{responseContent}";

                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        var problemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent);
                        return new ClientBadRequestException(problemDetails!, responseData);
                    }

                    if (response.StatusCode == HttpStatusCode.NotFound)
                        return new ClientNotFoundException(HttpStatusCode.NotFound, responseData);

                    if (response.StatusCode == HttpStatusCode.NotAcceptable)
                        return new ClientNotAcceptableException(HttpStatusCode.NotAcceptable, responseData);

                    return new ClientException(response.StatusCode, responseData);
                },
                HttpRequestMessageOptions = new Dictionary<string, object>
                {
                    { "IsThirdParty", isThirdParty },
                    { "ServiceName", serviceName }
                },
                ContentSerializer = new SystemTextJsonContentSerializer(GetJsonSerializerOptions()),
                UrlParameterFormatter = new GrillBotUrlParameterFormatter()
            })
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(uri);
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .AddHttpMessageHandler<Common.Handlers.HttpClientHandler>()
            .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(2, _ => TimeSpan.FromSeconds(5)));
    }

    public static void AddExternalServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<Common.Handlers.HttpClientHandler>();

        services.RegisterService<AuditLog.IAuditLogServiceClient>(configuration);
        services.RegisterService<Emote.IEmoteServiceClient>(configuration);
        services.RegisterService<ImageProcessing.IImageProcessingClient>(configuration);
        services.RegisterService<PointsService.IPointsServiceClient>(configuration);
        services.RegisterService<RubbergodService.IRubbergodServiceClient>(configuration);
        services.RegisterService<UserMeasures.IUserMeasuresServiceClient>(configuration);
        services.RegisterService<RemindService.IRemindServiceClient>(configuration);
        services.RegisterService<SearchingService.ISearchingServiceClient>(configuration);
        services.RegisterService<Graphics.IGraphicsClient>(configuration);
        services.RegisterService<InviteService.IInviteServiceClient>(configuration);
        services.RegisterService<UserManagementService.IUserManagementServiceClient>(configuration);
        services.RegisterService<MessageService.IMessageServiceClient>(configuration);
        services.RegisterService<UnverifyService.IUnverifyServiceClient>(configuration);
    }

    private static JsonSerializerOptions GetJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        options.Converters.Add(new ObjectToInferredTypesConverter());

        return options;
    }
}
