using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VindiniumBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new CommandLineOptions();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                // Values are available here
            }
        }
    }
}
