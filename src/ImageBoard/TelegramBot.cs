using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ImageBoard
{
    public class TelegramBot
    {
        private readonly ILogger<TelegramBot> _logger;
        private readonly SavedSettings _settings;
        private readonly TelegramBotClient _botClient;
        
        private const string AddedAdmin = "Я добавил @{0} в админы.";
        private const string CantAddAdmin = "Я не могу добавить '{0}' в админы - это не похоже на имя пользователя.";

        private const string ChatAdded = "Яволь! Я запомнил этот чат как ФЗшный, буду пускать людей отсюда в ФЗЧЬ!";
        private const string AlreadyAdded = "Я уже видел этот ФЗшный чат!";
        private const string NoAccess = "У тебя нет власти надо мной!";

        private const string Help = @"Привет, <b>{0}</b>!
                            Этот бот поможет тебе получить доступ во frizchan - анонимную имиджборду для участников Френдзоны.
                            
                            Что я умею:
                            <b>/code</b> чтобы получить код и ссылку для входа
                            <b>/help</b> показать это сообщение";

        private const string AdminHelp = @"
                            <b>/register</b> добавить чат в список ФЗшных (людей из этого чата пускаем в ФЗЧЬ). Работает только в групповых чатах.
                            <b>/add_admin <i>@username</i></b> добавить <i>username</i> в список админов. Работает только в личке с ботом.";

        private const string Authorized = @"человек прошёл проверку
                            Твой код для доступа во frizchan
                            {0}
                            Портал в <a href=""https://frizchan.ru/"">ФЗЧЬ</a>
                            Этот код доступа будет работать ещё {1:HH:mm:ss}";

        private const string NotAuthorized = @"человек не прошёл проверку
                                Скорее всего, ты ещё не участвовал во Френдзоне. Ты сможешь получить доступ к борде как только сыграешь хотя бы в одном сезоне!
                                Присоединиться к Френдзоне https://granumsalis.ru/friend_zone
                                Если ты уже играл, но всё равно не можешь зайти - смело пиши @DmAstr, он тебе обязательно поможет!";

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
                    bool isAdmin = _settings.Admins.Contains(e.Message.From.Username);

                    if (e.Message.Text == "/register" && e.Message.Chat.Type != ChatType.Private)
                    {
                        _logger.LogInformation($"{e.Message.From} tried to register chat {e.Message.Chat.Title} / {e.Message.Chat.Id}");
                        var chatId = e.Message.Chat.Id;
                        if (isAdmin)
                        {
                            if (!_settings.SavedChatIds.Contains(chatId))
                            {
                                _settings.AddChat(chatId);
                                await _botClient.SendTextMessageAsync(chatId,
                                    ChatAdded);
                                _logger.LogInformation($"Chat saved as FZ-friendly: {chatId}");
                            }
                            else
                            {
                                await _botClient.SendTextMessageAsync(chatId, AlreadyAdded);
                            }
                        }
                        else
                        {
                            await _botClient.SendTextMessageAsync(chatId, NoAccess);
                        }
                    }
                    
                    if (e.Message.Chat.Type == ChatType.Private)
                    {
                        var replyMarkup =
                            new ReplyKeyboardMarkup(new[] {"/code", "/help"}.Select(v => new KeyboardButton(v)));
                        
                        if (e.Message.Text == "/start" || e.Message.Text == "/help")
                        {
                            var message = string.Format(Help, e.Message.From.FirstName ?? e.Message.From.LastName ?? e.Message.From.Username);
                            if (isAdmin)
                            {
                                message += AdminHelp;
                            }
                            await _botClient.SendTextMessageAsync(e.Message.Chat, message, ParseMode.Html, replyMarkup: replyMarkup);
                        }
                        else if (e.Message.Text == "/add_admin")
                        {
                            var text = e.Message.Text.Substring("/add_admin".Length);

                            text = text.Trim().TrimStart('@');

                            if (text.Any(char.IsWhiteSpace))
                            {
                                await _botClient.SendTextMessageAsync(e.Message.Chat, string.Format(CantAddAdmin, text));
                            }
                            else
                            {
                                _logger.LogInformation($"Added admin: {text}");
                                _settings.AddAdmin(text);
                                await _botClient.SendTextMessageAsync(e.Message.Chat, string.Format(AddedAdmin, text));
                            }
                        }
                        else if (e.Message.Text == "/code")
                        {
                            bool sendPin = false;
                            foreach (var v in _settings.SavedChatIds)
                            {
                                var isUser = await _botClient.GetChatMemberAsync(v, e.Message.From.Id);
                                if (isUser.Status == ChatMemberStatus.Administrator ||
                                    isUser.Status == ChatMemberStatus.Creator ||
                                    isUser.Status == ChatMemberStatus.Member)
                                {
                                    sendPin = true;
                                    break;
                                }
                            }

                            _logger.LogInformation($"{e.Message.From} tried to get pin. Decision: {sendPin}");
                            if (sendPin)
                            {
                                var diff = Startup.CurrentTokenValidTill - DateTime.Now;
                                await _botClient.SendTextMessageAsync(e.Message.Chat.Id,
                                    string.Format(Authorized, Startup.CurrentToken, diff), ParseMode.Html, replyMarkup: replyMarkup);
                            }
                            else
                            {
                                await _botClient.SendTextMessageAsync(e.Message.Chat.Id, NotAuthorized, ParseMode.Html, replyMarkup: replyMarkup);
                            }
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