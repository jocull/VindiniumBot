using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VindiniumCore.GameTypes
{
    public class Game
    {
        #region Core Properties

        /// <summary>
        /// Unique identifier of the game
        /// </summary>
        [JsonProperty("id")]
        public string ID { get; set; }

        /// <summary>
        /// Current number of moves since the beginning.
        /// This is the total number of moves done at this point.
        /// Each turn contains 4 move (one for each player).
        /// So if you want to know the "real" turn number, you need to divide this number by 4.
        /// </summary>
        [JsonProperty("turn")]
        public int Turn { get; set; }

        /// <summary>
        /// Maximum number of turns. Same as above, you may need to divide this number by 4.
        /// </summary>
        [JsonProperty("maxTurns")]
        public int MaxTurns { get; set; }

        /// <summary>
        /// An array of Hero objects.
        /// </summary>
        [JsonProperty("heroes")]
        public Hero[] Heroes { get; set; }

        /// <summary>
        /// A Json object with two values...
        /// </summary>
        [JsonProperty("board")]
        public Board Board { get; set; }

        #endregion

        public static Game FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Game>(json);
        }
    }
}
