using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VindiniumCore.GameTypes
{
    public class GameState
    {
        #region Core Properties

        [JsonProperty("game")]
        public Game Game { get; set; }

        [JsonProperty("hero")]
        public Hero MyHero { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("viewUrl")]
        public string ViewUrl { get; set; }

        [JsonProperty("playUrl")]
        public string PlayUrl { get; set; }

        #endregion

        public static GameState FromJson(string json)
        {
            return JsonConvert.DeserializeObject<GameState>(json);
        }

        #region Helper methods

        public Tile FindMyHero()
        {
            return Game.FindHeroes(x => x.OwnerId == MyHero.ID).FirstOrDefault();
        }

        #endregion
    }
}
