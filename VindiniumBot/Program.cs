using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VindiniumBot.Bots;
using VindiniumCore;

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

                    while (true)
                    {
                        client.Run();

                        if (options.EndlessMode)
                        {
                            CoreHelpers.OutputLine("Sleeping for 15 seconds before starting again...");
                            Thread.Sleep(15 * 1000);
                        }
                        else
                        {
                            //End the program
                            return;
                        }
                    }
                }
                catch(Exception ex)
                {
                    CoreHelpers.OutputLine(ex.ToString());
                    throw;
                }
            }
        }
    }
}
