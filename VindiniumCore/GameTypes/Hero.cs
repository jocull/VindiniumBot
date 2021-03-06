﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VindiniumCore.GameTypes
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

        [JsonProperty("pos")]
        public Position Position { get; set; }

        /// <summary>
        /// This property is not always sent!
        /// </summary>
        [JsonProperty("lastDir")]
        public Directions? LastDirection { get; set; }

        [JsonProperty("life")]
        public int Life { get; set; }

        [JsonProperty("gold")]
        public int Gold { get; set; }

        [JsonProperty("spawnPos")]
        public Position SpawnPosition { get; set; }

        [JsonProperty("crashed")]
        public bool Crashed { get; set; }

        #endregion

        public static Hero FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Hero>(json);
        }

        public override string ToString()
        {
            string result = string.Format("{0} - {1:#,0} gold", Name, Gold);
            if (Crashed)
            {
                result += " (crashed)";
            }

            return result;
        }
    }
}
