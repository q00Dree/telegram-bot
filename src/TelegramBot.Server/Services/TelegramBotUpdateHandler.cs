using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.Server.Services;

public class TelegramBotUpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramBotUpdateHandler> _logger;

    public TelegramBotUpdateHandler(ITelegramBotClient botClient,
        ILogger<TelegramBotUpdateHandler> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _,
        Update update,
        CancellationToken cancellationToken)
    {
        var handler = update.Type switch
        {
            UpdateType.Message => BotOnMessageReceived(update.Message!, cancellationToken),
            _ => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handler;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient _,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Ошибка Telegram API:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogWarning("Возникла ошибка во время polling'а: {errorMessage}", errorMessage);

        if (exception is RequestException)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }

    private async Task BotOnMessageReceived(Message message, 
        CancellationToken cancellationToken)
    {
        await _botClient.SendTextMessageAsync(message.Chat.Id, message.Text ?? "Я получил твоё сообщение", cancellationToken: cancellationToken);
    }

    private Task UnknownUpdateHandlerAsync(Update update,
        CancellationToken _)
    {
        _logger.LogInformation("Unknown update type: {updateType}", update.Type);

        return Task.CompletedTask;
    }
}