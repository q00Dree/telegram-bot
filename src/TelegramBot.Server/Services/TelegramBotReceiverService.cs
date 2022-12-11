using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.Server.Services;

public class TelegramBotReceiverService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUpdateHandler _updateHandler;
    private readonly ILogger<TelegramBotReceiverService> _logger;

    public TelegramBotReceiverService(ITelegramBotClient botClient,
        IUpdateHandler updateHandler,
        ILogger<TelegramBotReceiverService> logger)
    {
        _botClient = botClient;
        _updateHandler = updateHandler;
        _logger = logger;
    }

    public async Task ReceiveAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = new[]
            {
                UpdateType.Message
            }
        };

        var me = await _botClient.GetMeAsync(stoppingToken);
        _logger.LogInformation("Начал начинать обновления для {botUsername}", me.Username ?? "undefined");

        await _botClient.ReceiveAsync(updateHandler: _updateHandler,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken);
    }
}
