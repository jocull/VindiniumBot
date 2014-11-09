using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VindiniumCore.PathFinding;

namespace VindiniumCore.GameTypes
{
    public class Position : IPosition
    {
        #region Core Properties

        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("X: {0}  Y: {1}", X, Y);
        }
    }
}
