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
        public Directions GetHeroMove(GameState state)
        {
            Game game = state.Game;
            Hero myHero = state.MyHero;

            CoreHelpers.OutputLine("");
            CoreHelpers.OutputLine("It's now turn {0:#,0} of {1:#,0}", game.Turn, game.MaxTurns);
            CoreHelpers.OutputLine("The hero has {0:#,0} HP", myHero.Life);
            CoreHelpers.OutputLine("The hero has {0:#,0} gold", myHero.Gold);
            CoreHelpers.OutputLine("The hero is at {0}, {1}", myHero.Position.X, myHero.Position.Y);

            const double goldMineTargetRatio = 0.25d; //More than your fair share!

            var safeTravelFunction = new Func<Node, int>(node =>
            {
                Tile t = node as Tile;
                var neighbors = state.Game.Board.GetNeighboringNodes(t, 2, true);
                bool avoid = neighbors.Select(x => x as Tile)
                                      .Any(nt =>
                {
                    if (nt.TileType == Tile.TileTypes.Hero
                            && nt.OwnerId != myHero.ID)
                    {
                        Hero h = game.LookupHero(nt);
                        if (h.Life > myHero.Life)
                        {
                            //CoreHelpers.OutputLine("*** AVOID HERO PATH {0} - {1}!", h.Name, h.Position);
                            return true; //Avoid stronger heros!
                        }
                    }
                    return false;
                });

                if (avoid)
                {
                    return 10;
                }

                return 1; //Default
            });

            //Find our hero and important items
            var myHeroTile = state.FindMyHero();
            var nearestTavern = game.FindPathsToTaverns(myHeroTile, x => true, safeTravelFunction).FirstOrDefault();
            var nearestUnownedGoldMine = game.FindPathsToGoldMines(myHeroTile, x => x.OwnerId != myHero.ID, safeTravelFunction).FirstOrDefault();
            var nearestNonPlayerHero = game.FindPathsToHeroes(myHeroTile, x => x.OwnerId != myHero.ID, safeTravelFunction).FirstOrDefault();
            var nearestNonPlayerHeroWithMines = game.FindPathsToHeroes(myHeroTile, x => x.OwnerId != myHero.ID && game.LookupGoldMinesForHero(x).Any(), safeTravelFunction).FirstOrDefault();
            var goldMineRatios = game.LookupGoldMineRatiosForHeros();

            //Don't get pinned by a spawn point!
            if (nearestNonPlayerHero != null
                && nearestNonPlayerHero.Distance == 1)
            {
                var hero = game.LookupHero(nearestNonPlayerHero.TargetNode as Tile);
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

            //If we're < 100, have the gold, and already by a tavern, let's just stay there and top off
            //Otherwise, if we're low health, go find the tavern and heal
            if (nearestTavern != null
                && (myHero.Life <= 20
                    || (myHero.Life < 90
                        && nearestTavern.Distance <= 1))
                && myHero.Gold >= 2)
            {
                CoreHelpers.OutputLine("Going for the nearest safe tavern! ({0}, {1})", nearestTavern.TargetNode.X, nearestTavern.TargetNode.Y);
                return nearestTavern.Directions.FirstOrDefault();
            }

            //Is anyone a little too rich?
            if (goldMineRatios.Any())
            {
                var ratio = goldMineRatios.Where(x => x.Key.ID != myHero.ID)
                                          .OrderByDescending(x => x.Value).First();
                if (ratio.Value >= goldMineTargetRatio)
                {
                    //Get the hero that we should gank
                    var targetHero = ratio.Key;
                    var targetHeroPath = game.FindPathsToHeroes(myHeroTile, x => x.OwnerId == ratio.Key.ID, safeTravelFunction).FirstOrDefault();

                    //Are we healthy enough to do it?
                    if (targetHeroPath != null
                        && nearestUnownedGoldMine != null
                        && myHero.Life > 80
                        && (myHero.Life - 20) > targetHero.Life
                        && (targetHeroPath.Distance < nearestUnownedGoldMine.Distance))
                    {
                        CoreHelpers.OutputLine("Going to kill {0} for the $$$! ({1}, {2})", targetHero.Name, targetHeroPath.TargetNode.X, targetHeroPath.TargetNode.Y);
                        return targetHeroPath.Directions.FirstOrDefault();
                    }
                }
            }

            //Is there a weaker hero nearby that we can attack?
            if (nearestUnownedGoldMine != null
                && nearestNonPlayerHeroWithMines != null
                && nearestNonPlayerHeroWithMines.Distance < nearestUnownedGoldMine.Distance)
            {
                var hero = game.LookupHero(nearestNonPlayerHeroWithMines.TargetNode as Tile);
                if (hero != null
                    && hero.Life <= (myHero.Life - 20))
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
            CoreHelpers.OutputLine("Snoozing...");
            return Directions.Stay;
        }
    }
}
