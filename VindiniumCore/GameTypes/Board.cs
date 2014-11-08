using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VindiniumCore.GameTypes
{
    public class Board
    {
        #region Core Properties

        /// <summary>
        /// The size of the map: the number of horizontal/vertical tiles.
        /// As the map is always a square, this number is the same for X and Y.
        /// </summary>
        [JsonProperty("size")]
        public int Size { get; set; }

        /// <summary>
        /// A string representing the map. 
        /// Each tile is coded using two chars (see the rules legend for more information).
        /// As you may already have noticed, to get each line of the map, you just have to use a %size (modulo) on the tiles.
        /// </summary>
        [JsonProperty("tiles")]
        public string TilesAsString { get; set; }

        /// <summary>
        /// If the game is finished or not.
        /// </summary>
        [JsonProperty("finished")]
        public bool Finished { get; set; }

        #endregion

        private Tile[][] _Tiles = null;
        public Tile[][] Tiles
        {
            get
            {
                if (_Tiles == null)
                {
                    _Tiles = Tile.ParseTileChars(Size, TilesAsString);
                }
                return _Tiles;
            }
        }

        /// <summary>
        /// Rebuild the board given our tiles
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var lineStrings = Tiles.Select(line =>
            {
                var rowStrings = line.Select(x => x.ToString());
                return string.Join("", rowStrings);
            });

            return string.Join(Environment.NewLine, lineStrings);
        }
    }
}
