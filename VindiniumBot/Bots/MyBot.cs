using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VindiniumCore;
using VindiniumCore.Bot;
using VindiniumCore.GameTypes;
using VindiniumCore.PathFinding;

namespace VindiniumBot.Bots
{
    internal class MyBot : IRobot
    {
        #region Properties

        private GameState _State { get; set; }
        private Game _Game { get; set; }
        private Board _Board { get; set; }
        private Hero _MyHero { get; set; }
        private Tile _MyHeroTile { get; set; }

        #endregion

        public void Reset()
        {
            //Release all references, clear lists, dictionaries, etc...
            _State = null;
            _Game = null;
            _Board = null;
            _MyHero = null;
            _MyHeroTile = null;
        }

        private void _Init(GameState state)
        {
            //Pre-calculate and cache important properties
            _State = state;
            _Game = state.Game;
            _Board = state.Game.Board;
            _MyHero = state.MyHero;
            _MyHeroTile = state.FindMyHero();
        }

        public Directions GetHeroMove(GameState state)
        {
            _Init(state);
            return _FindMove();
        }

        private Directions _FindMove()
        {
            CoreHelpers.OutputLine("");
            CoreHelpers.OutputLine("It's now turn {0:#,0} of {1:#,0}", _Game.Turn, _Game.MaxTurns);
            CoreHelpers.OutputLine("The hero has {0:#,0} HP", _MyHero.Life);
            CoreHelpers.OutputLine("The hero has {0:#,0} gold", _MyHero.Gold);
            CoreHelpers.OutputLine("The hero is at {0}, {1}", _MyHero.Position.X, _MyHero.Position.Y);

            const double goldMineTargetRatio = 0.255d; //More than your fair share!

            //Precalculate paths to other heros
            Tile myHeroTile = _State.FindMyHero();
            int mostPlayerGold = _Game.Heroes.Max(x => x.Gold);

            //How safe should we be traveling right now?
            var safeTravelFunction = new Func<Node, NodeStatus>(node =>
            {
                Tile t = node as Tile;
                var neighbors = _State.Game.Board.GetNeighboringNodes(t, 1, true).Select(x => x as Tile);

                foreach (var x in neighbors)
                {
                    //Any heros in the area?
                    if (x.TileType == Tile.TileTypes.Hero
                        && x.OwnerId != _MyHero.ID)
                    {
                        //Dangerous heros in the way?
                        Hero h = _Game.LookupHero(x);
                        if (h.Life > _MyHero.Life)
                        {
                            //Avoid!
                            return new NodeStatus(30, true);
                        }
                    }
                }
                return new NodeStatus(1, false);
            });

            //Where are the nearest safe taverns?
            var nearestTavern = _Game.FindPathsToTaverns(myHeroTile, statusFunc: safeTravelFunction).FirstOrDefault(); //The safe travel function here should avoid confrontations

            //Where's the nearest non-player hero?
            var nearestNonPlayerHero = _Game.FindPathsToHeroes(myHeroTile, x =>
            {
                if (x.OwnerId != _MyHero.ID)
                {
                    return true;
                }
                return false;
            }, safeTravelFunction).FirstOrDefault();

            //How about the nearest non-player hero with mines?
            var nearestNonPlayerHeroWithMines = _Game.FindPathsToHeroes(myHeroTile, x =>
            {
                if (x.OwnerId != _MyHero.ID
                    && _Game.LookupGoldMinesForHero(x).Any())
                {
                    //Don't consider heroes that are sitting by a tavern
                    var adjacentNodes = _Game.Board.GetNeighboringNodes(x, 1, false).Select(n => n as Tile);
                    if (adjacentNodes.Any(t => t.TileType == Tile.TileTypes.Tavern))
                    {
                        //We'll just get killed trying that...
                        return false;
                    }
                    return true;
                }
                return false;
            }, safeTravelFunction).FirstOrDefault();

            //Where's the nearest unowned gold mine?
            var nearestUnownedGoldMine = _Game.FindPathsToGoldMines(myHeroTile, x =>
            {
                //Block mines we own
                if (x.OwnerId != _MyHero.ID)
                {
                    //We'll get stuck if in this situation...
                    //          $-@2
                    //          @1[]
                    //Block mines that will get us stuck
                    var adjacentNodes = _Game.Board.GetNeighboringNodes(x, 1, true).Select(n => n as Tile);
                    if (adjacentNodes.Any(t => t.TileType == Tile.TileTypes.Hero && t.OwnerId != _MyHero.ID)) //enemy hero
                    {
                        //Don't consider mines that have a hero nearby...
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                return false;
            }, safeTravelFunction).FirstOrDefault();

            //Find the gold mine ratios
            var goldMineRatios = _Game.LookupGoldMineRatiosForHeros();

            //Don't get pinned by a spawn point!
            if (nearestNonPlayerHero != null
                && nearestNonPlayerHero.Distance == 1)
            {
                var hero = _Game.LookupHero(nearestNonPlayerHero.TargetNode as Tile);
                if (hero.SpawnPosition == hero.Position)
                {
                    //Get away somewhere worthwhile but not too close!
                    if (nearestUnownedGoldMine != null
                        && nearestUnownedGoldMine.Distance > 1)
                    {
                        CoreHelpers.OutputLine("Fleeing spawn to nearest gold mine! ({0}, {1})", nearestUnownedGoldMine.TargetNode.X, nearestUnownedGoldMine.TargetNode.Y);
                        return nearestUnownedGoldMine.Directions.FirstOrDefault();
                    }
                    else if (nearestTavern != null
                                && nearestTavern.Distance > 1)
                    {
                        CoreHelpers.OutputLine("Fleeing spawn to nearest tavern! ({0}, {1})", nearestTavern.TargetNode.X, nearestTavern.TargetNode.Y);
                        return nearestTavern.Directions.FirstOrDefault();
                    }
                }
            }

            //How safe should we be playing right now?
            int minimumHealth = 20;
            int nearestPlayerDistance = 99;
            Hero nearestPlayer = null;
            int myOwnedMinesCount = _Game.LookupGoldMinesForHero(myHeroTile).Count();
            if (nearestNonPlayerHero != null)
            {
                nearestPlayer = _Game.LookupHero(nearestNonPlayerHero.TargetNode as Tile);
                nearestPlayerDistance = nearestNonPlayerHero.Distance;
            }

            //There's no need to play it *THAT* safe if there isn't somebody nearby...
            if (myOwnedMinesCount > 1
                && nearestPlayerDistance <= 4
                && nearestPlayer != null
                && nearestPlayer.Life >= _MyHero.Life)
            {
                minimumHealth = 40;
            }
            else if (myOwnedMinesCount > 3
                     && nearestPlayerDistance <= 4
                     && nearestPlayer != null
                     && nearestPlayer.Life >= _MyHero.Life)
            {
                //This will likely lead us to kiting back to taverns
                minimumHealth = 50;
            }

            //If we're not all the way healed, have the gold, and already by a tavern, let's just stay there and top off
            //Otherwise, if we're low health, go find the tavern and heal
            if (nearestTavern != null
                && (_MyHero.Life <= minimumHealth
                    || (_MyHero.Life < 90
                        && nearestTavern.Distance <= 1))
                && _MyHero.Gold >= 2)
            {
                CoreHelpers.OutputLine("Going for the nearest safe tavern! ({0}, {1})", nearestTavern.TargetNode.X, nearestTavern.TargetNode.Y);
                return nearestTavern.Directions.FirstOrDefault();
            }

            //Is anyone a little too rich?
            if (goldMineRatios.Any())
            {
                var ratio = goldMineRatios.OrderByDescending(x => x.Value).First();
                if (ratio.Value >= goldMineTargetRatio)
                {
                    //Is this hero us?
                    if (ratio.Key.ID == _MyHero.ID)
                    {
                        //Is there a weaker hero nearby?
                        if (nearestNonPlayerHeroWithMines != null
                            && nearestNonPlayerHeroWithMines.Distance <= 4)
                        {
                            //Go get 'em, tiger!
                            Hero targetHero = _Game.LookupHero(nearestNonPlayerHeroWithMines.TargetNode as Tile);
                            if (targetHero.Life < _MyHero.Life)
                            {
                                CoreHelpers.OutputLine("Taking down opportunity target, {0}! ({1}, {2})", targetHero.Name, nearestNonPlayerHeroWithMines.TargetNode.X, nearestNonPlayerHeroWithMines.TargetNode.Y);
                                return nearestNonPlayerHeroWithMines.Directions.FirstOrDefault();
                            }
                        }
                        if (nearestTavern != null
                            && _MyHero.Gold >= mostPlayerGold)
                        {
                            //Is there another gold mine nearby we can get easily?
                            if (nearestTavern.Distance <= 3
                                && nearestUnownedGoldMine != null
                                && nearestUnownedGoldMine.Distance <= 4 //Extra space so we'll capture it by moving "into it"...
                                && _MyHero.Life >= 60)
                            {
                                CoreHelpers.OutputLine("Going to snag gold mine near camp site! ({0}, {1})", nearestUnownedGoldMine.TargetNode.X, nearestUnownedGoldMine.TargetNode.Y);
                                return nearestUnownedGoldMine.Directions.FirstOrDefault();
                            }

                            //Well camping is always nice thought!
                            CoreHelpers.OutputLine("Going to camp (tavern) because we are winning! ({0}, {1})", nearestTavern.TargetNode.X, nearestTavern.TargetNode.Y);
                            if (nearestTavern.Distance == 1
                                && _MyHero.Life > 50)
                            {
                                //We're healthy. No reason to spend the gold.
                                return Directions.Stay;
                            }
                            else
                            {
                                return nearestTavern.Directions.FirstOrDefault();
                            }
                        }
                    }
                    else
                    {
                        //Get the hero that we should gank
                        Hero targetHero = ratio.Key;
                        var targetHeroTile = _Game.FindTiles(x => x.TileType == Tile.TileTypes.Hero && x.OwnerId == targetHero.ID).FirstOrDefault();
                        var targetHeroPath = _Game.FindPathsToHeroes(myHeroTile, x => x.OwnerId == ratio.Key.ID, safeTravelFunction).FirstOrDefault();
                        var targetHeroPathToTavern = _Game.FindPathsToTaverns(targetHeroTile, x => true).FirstOrDefault();

                        //Are we healthy enough to do it?
                        if (targetHeroPath != null
                            && nearestUnownedGoldMine != null
                            && _MyHero.Life >= 50
                            && targetHero.Life < _MyHero.Life
                            && (targetHeroPath.Distance < (nearestUnownedGoldMine.Distance + 2)))
                        {
                            if (targetHeroPathToTavern == null
                                || targetHeroPathToTavern.Distance > 1)
                            {
                                //Don't attack players right by taverns
                                CoreHelpers.OutputLine("Going to kill {0} for the $$$! ({1}, {2})", targetHero.Name, targetHeroPath.TargetNode.X, targetHeroPath.TargetNode.Y);
                                return targetHeroPath.Directions.FirstOrDefault();
                            }
                        }
                    }
                }
            }

            //Is there a weaker hero nearby that we can attack?
            if (nearestUnownedGoldMine != null
                && nearestNonPlayerHeroWithMines != null
                && nearestNonPlayerHeroWithMines.Distance < nearestUnownedGoldMine.Distance)
            {
                var hero = _Game.LookupHero(nearestNonPlayerHeroWithMines.TargetNode as Tile);
                if (hero != null
                    && hero.Life < _MyHero.Life)
                {
                    //Go for it
                    CoreHelpers.OutputLine("Going to pick off {0}! ({1}, {2})", hero.Name, hero.Position.X, hero.Position.Y);
                    return nearestNonPlayerHeroWithMines.Directions.FirstOrDefault();
                }
            }

            //Find the nearest unowned gold mine and try to capture it
            if (nearestUnownedGoldMine != null)
            {
                CoreHelpers.OutputLine("Going for the nearest gold mine! ({0}, {1})", nearestUnownedGoldMine.TargetNode.X, nearestUnownedGoldMine.TargetNode.Y);
                return nearestUnownedGoldMine.Directions.FirstOrDefault();
            }

            //Don't go anywhere if there's truly nothing to do...
            CoreHelpers.OutputLine("Snoozing... (probably blocked!)");
            return Directions.Stay;
        }
    }
}
