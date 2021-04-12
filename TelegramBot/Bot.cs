using System;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TestConsole.Services;

namespace TelegramBot
{
    public class Bot
    {
        ITelegramBotClient botClient;

        const string Start = "/start";
        const string IncorrectInput = "Sorry, but the data entered is incorrect. Please try something like 31.12.2014 - EUR";
        const string HelloText = "I can help you find out what exchange rates used to be.\nSelect date and currency. For example 31.12.2014 - EUR";

        private static readonly AutoResetEvent _closingEvent = new AutoResetEvent(false);

        public async Task RunAsync()
        {
            botClient = new TelegramBotClient(ConfigurationManager.AppSettings.Get("Token")) 
                { Timeout = TimeSpan.FromSeconds(Int32.Parse(ConfigurationManager.AppSettings.Get("BotTimeout"))) };

            var me = await botClient.GetMeAsync();
            Console.WriteLine($"User {me.Id} - {me.FirstName} was launched at {DateTime.Now}.");

            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();

            Console.WriteLine("Press Ctrl + C to cancel!");
            //CancelKey need to run docker without "tail -f /dev/null" parameter
            Console.CancelKeyPress += ((s, a) =>
            {
                Console.WriteLine("Bye!");
                _closingEvent.Set();
            });
            _closingEvent.WaitOne();

            botClient.StopReceiving();
        }
        
        async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            ShowLog(e.Message);

            if (e.Message.Text == null) return;

            if (e.Message.Text == Start)
            {
                await SayHello(e);
                return;
            }

            var FormattedRequest = GetFormattedRequest(e);
            if (GetValidation(FormattedRequest))
            {
                var RequestApiPB = new PrivatBank(FormattedRequest[0], FormattedRequest[1]);
                if (RequestApiPB.IHaveExchangeRate())
                {
                    var ExchangeRate = RequestApiPB.GetExchangeRate();
                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: ExchangeRate);
                    return;
                }

                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "" +
                        "I do not have exchange rates for " + FormattedRequest[0] + " Try another date.");
                return;
            }

            await SendTextMessageWithQuotationAsync(e, IncorrectInput);
        }

        async Task SendTextMessageWithQuotationAsync(MessageEventArgs e, string text)
        {
            await botClient.SendTextMessageAsync(chatId: e.Message.Chat,
                text: text,
                parseMode: ParseMode.Markdown,
                replyToMessageId: e.Message.MessageId);
        }

        async Task SayHello(MessageEventArgs e)
        {
            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: HelloText);
        }

        string[] GetFormattedRequest(MessageEventArgs e)
        {
            var args = e.Message.Text.Split("-", StringSplitOptions.RemoveEmptyEntries);

            if(DateTime.TryParse(args[0], out DateTime t))
                args[0] = t.ToString(CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.ShortDatePattern);
            else
                args[0] = DateTime.Now.ToString(CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.ShortDatePattern);

            if (args.Length > 1 && args[1] is string)
                args[1] = GetLetterString(args[1]).ToUpper();

            return args;
        }
        public string GetLetterString(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
                if (Char.IsLetter(c))
                    sb.Append(c);

            return sb.ToString();
        }
        bool GetValidation(string[] FormattedRequest)
        {
            if (FormattedRequest.Length == 2 && DateTime.TryParse(FormattedRequest[0], out DateTime t)
                && FormattedRequest[1] is string && FormattedRequest[1].Length == 3)
                return true;

            return false;

        }
        void ShowLog(Message m)
        {
            Console.WriteLine($"{m.From.FirstName} sent message {m.MessageId} to chat {m.Chat.Id} at {m.Date}. ");
        }
    }
}
