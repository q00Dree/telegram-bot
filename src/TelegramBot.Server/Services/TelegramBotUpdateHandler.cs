using System.Net;
using System.Net.Http;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

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
        if (message.Text is not { } messageText)
        {
            return;
        }

        var action = messageText.Split(' ')[0] switch
        {
            "/random_dad_joke" => SendRandomDadJoke(_botClient, message, cancellationToken),
            _ => Usage(_botClient, message, cancellationToken)
        };

        await action;
    }

    private Task UnknownUpdateHandlerAsync(Update update,
        CancellationToken _)
    {
        _logger.LogInformation("Unknown update type: {updateType}", update.Type);

        return Task.CompletedTask;
    }

    private static async Task SendRandomDadJoke(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient(new HttpClientHandler()
        {
            Proxy = new WebProxy()
            {
                Address = new Uri("http://52.45.139.115:80")
            },
            UseProxy = true
        });

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/plain");

        var response = await httpClient.GetAsync("https://icanhazdadjoke.com/");
        var joke = await response.Content.ReadAsStringAsync(cancellationToken);

        await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: joke,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
    }

    private static async Task Usage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        const string usage = "Usage:\n" +
                             "/random_dad_joke - send random dad's joke";

        await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: usage,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
    }
}