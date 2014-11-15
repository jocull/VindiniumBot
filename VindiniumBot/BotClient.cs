using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VindiniumCore;
using VindiniumCore.Bot;
using VindiniumCore.GameTypes;

namespace VindiniumBot
{
    internal class BotClient
    {
        public CommandLineOptions Options { get; private set; }
        public IRobot Bot { get; private set; }
        private Stopwatch _Timer { get; set; }

        public BotClient(CommandLineOptions options, IRobot bot)
        {
            this.Options = options;
            this.Bot = bot;
            this._Timer = new Stopwatch();
        }

        public void Run()
        {
            if (this.Options.BenchmarkMode)
            {
                CoreHelpers.OutputLine("Running benchmark mode... This will never end.");
                CoreHelpers.OutputLine("You should expect times from each run to be fairly consistent.");
                CoreHelpers.OutputLine("Large differences in times could indicate a poorly shared server.");
                _Benchmark(); //Never ends...
                return;
            }

            //Reset the brain before the game starts
            Bot.Reset();

            GameState state = null;
            try
            {
                state = _BeginGame();
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    throw new TimeoutWebException("Timed out while searching for game...");
                }
                throw; //Rethrow
            }

            while (!state.Game.Finished)
            {
                //Get and send the next move
                _Timer.Restart();
                Directions move = Bot.GetHeroMove(state);
                CoreHelpers.OutputLine("Calculate move: {0:#,0} ms", _Timer.ElapsedMilliseconds);

                state = _SendMove(state, move);
            }

            //At the end of the game, report the outcome
            var heroRankings = state.Game.Heroes.OrderByDescending(x => x.Gold);
            CoreHelpers.OutputLine("Final scores:");
            CoreHelpers.OutputLine("=======================");
            foreach (var h in heroRankings)
            {
                CoreHelpers.OutputLine(h.ToString());
            }
            CoreHelpers.OutputLine("");
        }

        private string _PostMessage(string url, NameValueCollection data)
        {
            try
            {
                using (var wc = new BotWebClient())
                {
                    
                    var responseBytes = wc.UploadValues(url, "POST", data);
                    var responseString = Encoding.UTF8.GetString(responseBytes);

                    return responseString;
                }
            }
            catch (WebException ex)
            {
                int httpCode = 0;
                string httpResponse = "";
                if (ex.Response != null)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null)
                    {
                        httpCode = (int)response.StatusCode;
                        using (StreamReader sr = new StreamReader(ex.Response.GetResponseStream()))
                        {
                            httpResponse = sr.ReadToEnd();
                        }
                    }
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("A web exception occurred!");
                sb.AppendLine("HTTP Code: " + httpCode);
                sb.AppendLine("HTTP Response:");
                sb.AppendLine(httpResponse);
                sb.AppendLine();
                sb.AppendLine("Exception details:");
                sb.AppendLine(ex.ToString());

                throw new ApplicationException(sb.ToString(), ex);
            }
        }

        private GameState _GameStateFromJson(string json)
        {
            //Decode and return the GameState
            GameState state = GameState.FromJson(json);
            if (state == null)
            {
                throw new ApplicationException("Failed to parse initial GameState JSON:" + Environment.NewLine + json);
            }
            return state;
        }

        private GameState _BeginGame()
        {
            //Generate the URL we will access
            var uri = new UriBuilder(Options.ServerUrl);
            uri.Path = Options.ArenaMode
                        ? "/api/arena"
                        : "/api/training";
            var url = uri.ToString();

            //Setup the values we will POST
            var data = new NameValueCollection();
            data["key"] = Options.PrivateKey;
            if (!Options.ArenaMode)
            {
                data["turns"] = Options.NumberOfTurns.ToString();
            }
            if (!string.IsNullOrEmpty(Options.Map))
            {
                data["map"] = Options.Map;
            }

            //Do it
            CoreHelpers.OutputLine("Searching for new game...");
            var json = _PostMessage(url, data);

            //Decode and return the GameState
            GameState state = _GameStateFromJson(json);
            CoreHelpers.OutputLine("Got new game state!");
            CoreHelpers.OutputLine("View at " + state.ViewUrl);

            if (Debugger.IsAttached)
            {
                new Thread(new ThreadStart(() =>
                    {
                        Process.Start(state.ViewUrl);
                    })).Start();
            }

            return state;
        }

        private GameState _SendMove(GameState state, Directions move)
        {
            var data = new NameValueCollection();
            data["key"] = Options.PrivateKey;
            data["dir"] = move.ToString();

            _Timer.Restart();
            CoreHelpers.OutputLine("Sending move: " + move);
            string json = _PostMessage(state.PlayUrl, data);
            CoreHelpers.OutputLine("Response time: {0:#,0} ms", _Timer.ElapsedMilliseconds);

            _Timer.Restart();
            GameState newState = _GameStateFromJson(json);
            CoreHelpers.OutputLine("Parsing time: {0:#,0} ms", _Timer.ElapsedMilliseconds);

            return newState;
        }

        private void _Benchmark()
        {
            Stopwatch sw = new Stopwatch();
            while (true)
            {
                sw.Restart();

                int i = 0;
                const int iMax = 1000000000;
                while (i < iMax)
                {
                    i++;
                }

                CoreHelpers.OutputLine("{0:#,0} iterations in {1:#,0} ms", iMax, sw.ElapsedMilliseconds);
            }
        }
    }
}
