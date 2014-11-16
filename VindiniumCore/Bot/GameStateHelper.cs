using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VindiniumCore.GameTypes;
using VindiniumCore.PathFinding;

namespace VindiniumCore.Bot
{
    public class GameStateHelper
    {
        public GameState State { get; private set; }
        public Game Game { get; private set; }
        public Board Board { get; private set; }
        public Hero MyHero { get; private set; }
        public Tile MyHeroTile { get; private set; }
        public int MostGoldInGame { get; private set; }
        public bool WillWinGame { get; set; }
        public IDictionary<Hero, int> HeroEndGameGold { get; set; }
        public Func<Tile, bool> OwnedTile { get; private set; }
        public Func<Tile, bool> UnownedTile { get; private set; }
        public IEnumerable<Tile> GoldMinesOwned { get; private set; }
        public IEnumerable<Tile> GoldMinesOpen { get; private set; }
        public IEnumerable<Tile> GoldMinesEnemy { get; private set; }
        public IEnumerable<Tile> GoldMinesUnowned { get; private set; }
        public IEnumerable<Tile> TavernTiles { get; private set; }
        public IEnumerable<Tile> EnemyHeroTiles { get; private set; }
        public Func<Node, NodeStatus> TravelModeBlockingHeroes { get; private set; }
        public Func<Node, NodeStatus> TravelModeAvoidingHeroes { get; private set; }

        public GameStateHelper(GameState state)
        {
            //Pre-calculate and cache important properties
            State = state;
            Game = state.Game;
            Board = state.Game.Board;
            MyHero = state.MyHero;
            MyHeroTile = state.FindMyHero();
            MostGoldInGame = state.Game.Heroes.Max(x => x.Gold);

            //Setup ownership functions
            OwnedTile = t => t.OwnerId == MyHero.ID;
            UnownedTile = t => t.OwnerId != MyHero.ID;

            //Find all the gold mines owned by us
            var goldMines = state.Game.FindGoldMines().ToList(); //Pre-filter tiles
            GoldMinesOwned = goldMines.Where(OwnedTile).ToList();
            GoldMinesOpen = goldMines.Where(x => !x.OwnerId.HasValue).ToList();
            GoldMinesEnemy = goldMines.Where(x => x.OwnerId.HasValue && x.OwnerId != MyHero.ID).ToList();
            GoldMinesUnowned = goldMines.Where(UnownedTile).ToList();

            //Figure out who will be winning the game currently
            var winningHeros = Game.Heroes.Select(h =>
            {
                var mines = goldMines.Where(g => g.OwnerId == h.ID).Count();
                int endGold = h.Gold + ((Game.MaxTurns - Game.Turn) * mines);
                return new { Hero = h, EndGold = endGold };
            })
            .OrderByDescending(x => x.EndGold)
            .ToList();

            HeroEndGameGold = winningHeros.ToDictionary(x => x.Hero, x => x.EndGold);
            if (winningHeros.First().EndGold > 0
                && winningHeros.First().EndGold != winningHeros.Skip(1).First().EndGold)
            {
                WillWinGame = winningHeros.First().Hero.ID == MyHero.ID;
            }

            //Find all taverns
            TavernTiles = state.Game.FindTaverns().ToList();

            //Find enemy players
            EnemyHeroTiles = state.Game.FindHeroes(x => x.OwnerId != MyHero.ID).ToList();

            //Setup travel modes
            TravelModeBlockingHeroes = new Func<Node, NodeStatus>(node =>
            {
                Tile t = node as Tile;
                var neighbors = State.Game.Board.GetNeighboringNodes(t, 1, true).Select(x => x as Tile);

                foreach (var x in neighbors)
                {
                    //Any heros in the area?
                    if (x.TileType == Tile.TileTypes.Hero
                        && x.OwnerId != MyHero.ID)
                    {
                        //Dangerous heros in the way?
                        Hero h = Game.LookupHero(x);
                        if (h.Life > MyHero.Life)
                        {
                            //Avoid!
                            return new NodeStatus(30, true);
                        }
                    }
                }
                return new NodeStatus(1, false);
            });

            TravelModeAvoidingHeroes = new Func<Node, NodeStatus>(node =>
            {
                Tile t = node as Tile;
                var neighbors = State.Game.Board.GetNeighboringNodes(t, 2, true).Select(x => x as Tile);

                foreach (var x in neighbors)
                {
                    //Any heros in the area?
                    if (x.TileType == Tile.TileTypes.Hero
                        && x.OwnerId != MyHero.ID)
                    {
                        //Dangerous heros in the way?
                        Hero h = Game.LookupHero(x);
                        if (h.Life > MyHero.Life)
                        {
                            //Avoid!
                            return new NodeStatus(20, false);
                        }
                    }
                }
                return new NodeStatus(1, false);
            });
        }
    }
}
