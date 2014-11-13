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

        public BotClient(CommandLineOptions options, IRobot bot)
        {
            this.Options = options;
            this.Bot = bot;
        }

        public void Run()
        {
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
                Directions move = Bot.GetHeroMove(state);
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

            CoreHelpers.OutputLine("Sending move: " + move);
            string json = _PostMessage(state.PlayUrl, data);
            return _GameStateFromJson(json);
        }
    }
}
