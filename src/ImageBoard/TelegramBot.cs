using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace ImageBoard
{
    public class TelegramBot
    {
        private readonly ILogger<TelegramBot> _logger;
        private readonly TelegramBotClient _botClient;

        public TelegramBot(ILogger<TelegramBot> logger)
        {
            _logger = logger;
            _botClient = new TelegramBotClient(Startup.TelegramToken);
            _botClient.OnMessage += Handler;
        }

        private async void Handler(object sender, MessageEventArgs e)
        {
            try
            {
                if (e.Message.Text != null)
                {
                    // await _botClient.SendTextMessageAsync(e.Message.Chat.Id, "Hello world");
                    // await _botClient.SendTextMessageAsync(e.Message.Chat.Id, JsonConvert.SerializeObject(e));
                    _logger.LogInformation("Replied to TG message " + (e.Message.From.Username ?? "<unknown>"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle telegram message");
            }
        }

        public void Start()
        {
            _botClient.StartReceiving();
        }
    }
}