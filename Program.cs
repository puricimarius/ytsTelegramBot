using System.Collections.Generic;
using YtsClient;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace YtsTelegramBot
{
    class Program
    {
        static readonly string API_KEY = "The Api Key";
        static readonly TelegramBotClient Bot = new TelegramBotClient(API_KEY);

        static void Main(string[] args)
        {
            string username = Bot.GetMeAsync().Result.Username;
            System.Console.Title = username;

            Bot.OnMessage += OnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            Bot.StartReceiving();
            System.Console.WriteLine($"The Bot {username} has started receiving messages");
            System.Console.WriteLine("Press Q to quit...");
            while (true)
            {
                System.ConsoleKeyInfo cki = System.Console.ReadKey(true);
                if(cki.Key == System.ConsoleKey.Q) break;
            }
            Bot.StopReceiving();
            System.Console.WriteLine($"The Bot {username} has stopped receiving messages");
        }

        static async void OnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.Text) return;

            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            switch (message.Text)
            {
                case "/start":
                case "/help":
                    await Bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Welcome to ytsTelegramBot by Markus_17\n" +
                        "Send me any movie title and I will try to find it for you\n" +
                        "Send /help to see this message again"
                    );
                    break;
                default:
                    var res = await Request.Make(query_term: message.Text, page: 1, limit: 50);

                    if(res.data.movie_count == 0)
                    {
                        await Bot.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "No such movie was found..."
                        );
                        return;
                    }

                    string output = "";
                    var buttons = new List<InlineKeyboardButton>();
                    for (int i = 0; i < res.data.movies.Count; i++)
                    {
                        output += $"{i+1}) {res.data.movies[i].title_long}\n";
                        buttons.Add(new InlineKeyboardButton{Text = $"{i+1}", CallbackData = $"{res.data.movies[0].id}"});
                    }

                    var inlineKeyboard = new InlineKeyboardMarkup(buttons.ToArray());

                    await Bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: output,
                        replyMarkup: inlineKeyboard
                    );
                    break;
            }
        } 

        static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;
            await Bot.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"ID {callbackQuery.Data}"
            );
            var resMovie = await Request.FindMovieById(callbackQuery.Data);

            var buttons = new List<InlineKeyboardButton>();
            for (int i = 0; i < resMovie.data.movie.torrents.Count; i++)
            {
                buttons.Add(new InlineKeyboardButton{Text = resMovie.data.movie.torrents[i].quality,  Url = resMovie.data.movie.torrents[i].url});
            }

            await Bot.SendPhotoAsync(
                chatId: callbackQuery.Message.Chat.Id,
                photo: resMovie.data.movie.large_cover_image,
                caption: resMovie.data.movie.description_full,
                replyMarkup: new InlineKeyboardMarkup(buttons.ToArray())
            );
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            System.Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message
            );
        }
    }
}