using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using TelegramBot.Server.Configurations;
using TelegramBot.Server.Services;

namespace TelegramBot.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddTelegramBotConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TelegramBotConfiguration>(configuration.GetSection(nameof(TelegramBotConfiguration)));
    }

    public static void AddTelegramBotClient(this IServiceCollection services)
    {
        services.AddHttpClient("telegram-bot-client")
            .AddTypedClient<ITelegramBotClient>((httpClient, serviceProvider) =>
            {
                var botConfiguration = serviceProvider
                    .GetRequiredService<IOptions<TelegramBotConfiguration>>()
                    .Value;

                return new TelegramBotClient(botConfiguration.Token, httpClient);
            });
    }

    public static void AddTelegramBotUpdateHandler(this IServiceCollection services)
    {
        services.AddScoped<IUpdateHandler, TelegramBotUpdateHandler>();
    }

    public static void AddTelegramBotReceiverService(this IServiceCollection services)
    {
        services.AddScoped<TelegramBotReceiverService>();
    }

    public static void AddTelegramBotPollingBackgroundService(this IServiceCollection services)
    {
        services.AddHostedService<TelegramBotPollingBackgroundService>();
    }
}