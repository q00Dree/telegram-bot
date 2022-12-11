namespace TelegramBot.Server.Services;

public class TelegramBotPollingBackgroundService : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<TelegramBotPollingBackgroundService> _logger;

	public TelegramBotPollingBackgroundService(IServiceProvider serviceProvider,
		ILogger<TelegramBotPollingBackgroundService> logger)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				using var scope = _serviceProvider.CreateScope();
				var receiver = scope.ServiceProvider.GetRequiredService<TelegramBotReceiverService>();

				await receiver.ReceiveAsync(stoppingToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Polling завершился с ошибкой!");

				await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
			}
		}
	}
}