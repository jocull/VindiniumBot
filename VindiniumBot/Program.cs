using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VindiniumBot.Bots;

namespace VindiniumBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new CommandLineOptions();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                try
                {
                    MyBot bot = new MyBot();
                    BotClient client = new BotClient(options, bot);
                    client.Run();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
            }
        }
    }
}
