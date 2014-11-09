using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VindiniumBot
{
    internal class CommandLineOptions
    {
        [Option('k',
                "key",
                Required = true,
                HelpText = "Your private key")]
        public string PrivateKey { get; set; }

        [Option('a',
                "arena",
                Required = false,
                DefaultValue = false,
                HelpText = "Play in the real arena?")]
        public bool ArenaMode { get; set; }

        [Option('t',
                "turns",
                Required = false,
                DefaultValue = (uint)100,
                HelpText = "Number of turns to run in training mode")]
        public uint NumberOfTurns { get; set; }

        [Option('m',
                "map",
                Required = false,
                DefaultValue = null,
                HelpText = "A custom map to run in training mode")]
        public string Map { get; set; }

        [Option('u',
                "url",
                Required = false,
                DefaultValue = "http://vindinium.org",
                HelpText = "HTTP URL of Vindinium server")]
        public string ServerUrl { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
