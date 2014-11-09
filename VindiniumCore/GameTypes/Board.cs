using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VindiniumCore.PathFinding;

namespace VindiniumCore.GameTypes
{
    public class Board : INodeSet
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

        #endregion

        private Tile[][] _Tiles = null;
        public Tile[][] Tiles
        {
            get
            {
                if (_Tiles == null)
                {
                    if (!string.IsNullOrEmpty(TilesAsString))
                    {
                        _Tiles = Tile.ParseTileChars(Size, TilesAsString);
                    }
                }
                return _Tiles;
            }
        }

        public IEnumerable<Tile> TilesFlattened
        {
            get
            {
                if (Tiles == null)
                {
                    return Enumerable.Empty<Tile>();
                }

                return Tiles.SelectMany(x => x);
            }
        }

        #region ToString overrides

        /// <summary>
        /// Rebuild the board given our tiles
        /// </summary>
        public override string ToString()
        {
            var lineStrings = Tiles.Select(line =>
            {
                var rowStrings = line.Select(x => x.ToString());
                return string.Join("", rowStrings);
            });

            return string.Join(Environment.NewLine, lineStrings);
        }

        /// <summary>
        /// Rebuild the board tiles with a visible pathing
        /// </summary>
        public string ToString(DirectionSet directions)
        {
            var lineStrings = Tiles.Select(line =>
            {
                var rowStrings = line.Select(tile =>
                {
                    if (directions != null
                        && directions.NodeSequence.Any(p => p == tile))
                    {
                        return "++";
                    }
                    else
                    {
                        return tile.ToString();
                    }
                });
                return string.Join("", rowStrings);
            });

            return string.Join(Environment.NewLine, lineStrings);
        }

        #endregion

        #region INodeSet implementation

        public Node GetNode(int x, int y)
        {
            if (x < 0
                || y < 0
                || x > (Size - 1)
                || y > (Size - 1))
            {
                return null;
            }

            return Tiles[y][x];
        }

        public Node GetRelativeNode(Node baseNode, int x, int y)
        {
            //Apply the adjustment and search
            return GetNode(baseNode.X + x,
                           baseNode.Y + y);
        }

        public IEnumerable<Node> GetAllNodes()
        {
            return TilesFlattened;
        }

        #endregion
    }
}
