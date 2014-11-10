using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VindiniumCore.PathFinding;

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

        /// <summary>
        /// If the game is finished or not.
        /// </summary>
        [JsonProperty("finished")]
        public bool Finished { get; set; }

        #endregion

        public static Game FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Game>(json);
        }

        #region Pathfinding properties

        private PathFinder _Pathfinder;
        public PathFinder Pathfinder
        {
            get
            {
                if (_Pathfinder == null)
                {
                    _Pathfinder = new PathFinder(Board);
                }
                return _Pathfinder;
            }
        }
        
        #endregion

        #region Finding tiles

        public IEnumerable<Tile> FindTiles(Func<Tile, bool> predicate)
        {
            return Board.TilesFlattened.Where(x => predicate(x));
        }

        public IEnumerable<Tile> FindHeroes(Func<Tile, bool> predicate)
        {
            return FindTiles(x => x.TileType == Tile.TileTypes.Hero).Where(x => predicate(x));
        }

        public IEnumerable<Tile> FindGoldMines(Func<Tile, bool> predicate)
        {
            return FindTiles(x => x.TileType == Tile.TileTypes.GoldMine).Where(x => predicate(x));
        }

        public IEnumerable<Tile> FindTaverns(Func<Tile, bool> predciate)
        {
            return FindTiles(x => x.TileType == Tile.TileTypes.Tavern).Where(x => predciate(x));
        }

        #endregion

        #region Finding paths

        public DirectionSet FindPath(Node source, Node target)
        {
            var path = Pathfinder.FindShortestPath(source, target);
            if (path != null)
            {
                return new DirectionSet(path);
            }
            return null;
        }

        public IEnumerable<DirectionSet> FindPaths(Node source, IEnumerable<Tile> tiles)
        {
            return tiles.Select(tile => FindPath(source, tile))
                        .Where(x => x != null)
                        .OrderBy(x => x.Distance);
        }

        public IEnumerable<DirectionSet> FindPathsToHeroes(Node source, Func<Tile, bool> predicate)
        {
            return FindPaths(source, FindHeroes(predicate));
        }

        public IEnumerable<DirectionSet> FindPathsToGoldMines(Node source, Func<Tile, bool> predicate)
        {
            return FindPaths(source, FindGoldMines(predicate));
        }

        public IEnumerable<DirectionSet> FindPathsToTaverns(Node source, Func<Tile, bool> predicate)
        {
            return FindPaths(source, FindTaverns(predicate));
        }

        #endregion

        #region Lookups and helpers

        public Hero LookupHero(Tile tile)
        {
            return Heroes.FirstOrDefault(x => x.ID == tile.OwnerId);
        }

        #endregion
    }
}
