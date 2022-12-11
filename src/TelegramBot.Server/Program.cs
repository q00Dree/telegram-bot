using TelegramBot.Server.Extensions;

namespace TelegramBot.Server;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args)
            .Build()
            .Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddTelegramBotConfiguration(context.Configuration); 
                services.AddTelegramBotClient(); 
                services.AddTelegramBotUpdateHandler(); 
                services.AddTelegramBotReceiverService(); 
                services.AddTelegramBotPollingBackgroundService();
            });
    }
}