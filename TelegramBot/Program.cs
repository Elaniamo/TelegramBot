using System.Threading.Tasks;

namespace TelegramBot
{
    class Program
    {
        static async Task Main()
        {
            var Bot = new Bot();
            await Bot.RunAsync();
        }
    }
}

