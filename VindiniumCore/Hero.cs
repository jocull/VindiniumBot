using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VindiniumCore
{
    public class Hero
    {
        #region Core Properties

        [JsonProperty("id")]
        public byte ID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("userId")]
        public string UserID { get; set; }

        [JsonProperty("elo")]
        public int Elo { get; set; }

        [JsonProperty("position")]
        public Position Position { get; set; }

        /// <summary>
        /// This property is not always sent!
        /// </summary>
        [JsonProperty("lastDir")]
        public string LastDirection { get; set; }

        [JsonProperty("life")]
        public int Life { get; set; }

        [JsonProperty("gold")]
        public int Gold { get; set; }

        [JsonProperty("spawnPos")]
        public Position SpawnPosition { get; set; }

        [JsonProperty("crashed")]
        public bool Crashed { get; set; }

        #endregion
    }
}
