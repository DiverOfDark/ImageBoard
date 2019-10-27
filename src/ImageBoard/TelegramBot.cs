using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace ImageBoard
{
    public class TelegramBot
    {
        private readonly ILogger<TelegramBot> _logger;
        private readonly SavedSettings _settings;
        private readonly TelegramBotClient _botClient;

        public TelegramBot(ILogger<TelegramBot> logger, SavedSettings settings)
        {
            _logger = logger;
            _settings = settings;
            _botClient = new TelegramBotClient(Startup.TelegramToken);
            _botClient.OnMessage += Handler;
        }

        private async void Handler(object sender, MessageEventArgs e)
        {
            try
            {
                if (e.Message.Text != null)
                {
                    bool isAdmin = e.Message.From.Username == "diverofdark";

                    if (e.Message.Text == "/register" && e.Message.Chat.Type != ChatType.Private)
                    {
                        _logger.LogInformation($"{e.Message.From} tried to register chat {e.Message.Chat.Title} / {e.Message.Chat.Id}");
                        var chatId = e.Message.Chat.Id;
                        if (isAdmin)
                        {
                            _settings.AddChat(chatId);
                            await _botClient.SendTextMessageAsync(chatId, 
                                "Яволь! Я запомнил этот чат как ФЗшный, буду пускать людей отсюда в ФЗЧЬ!");
                            _logger.LogInformation($"Chat saved as FZ-friendly: {chatId}");
                        }
                        else
                        {
                            await _botClient.SendTextMessageAsync(chatId, "У тебя нет власти надо мной!");
                        }
                    }

                    if (e.Message.Chat.Type == ChatType.Private)
                    {
                        bool sendPin = false;
                        foreach (var v in _settings.SavedChatIds)
                        {
                            var isUser = await _botClient.GetChatMemberAsync(v, e.Message.From.Id);
                            _logger.LogInformation("Check response: " + JsonConvert.SerializeObject(isUser));
                            if (isUser.IsMember == true)
                            {
                                sendPin = true;
                                break;
                            }
                        }

                        _logger.LogInformation($"{e.Message.From} tried to get pin. Decision: {sendPin}");
                        if (sendPin)
                        {
                            await _botClient.SendTextMessageAsync(e.Message.Chat.Id,
                                $"Привет друг! Твой пропуск в ФЗЧЬ: {Startup.CurrentToken}. Этот пароль работает до {Startup.CurrentTokenValidTill:G}");
                        }
                        else
                        {
                            await _botClient.SendTextMessageAsync(e.Message.Chat.Id, "Ты не пройдёшь!");
                        }
                    }
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