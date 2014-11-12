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

            const double goldMineTargetRatio = 0.275d; //More than your fair share!

            //Precalculate paths to other heros
            Tile myHeroTile = state.FindMyHero();
            Dictionary<Hero, DirectionSet> heroPaths = game.FindPathsToHeroes(myHeroTile, t => t.OwnerId != myHeroTile.OwnerId)
                                                           .ToDictionary(x => game.LookupHero(x.TargetNode as Tile),
                                                                         x => x);

            var safeTravelFunction = new Func<Node, int>(node =>
            {
                Tile t = node as Tile;
                var neighbors = state.Game.Board.GetNeighboringNodes(t, 3, true).Select(x => x as Tile);

                int totalCost = 1;  //Default
                foreach (var x in neighbors)
                {
                    //Any heros in the area?
                    if (x.TileType == Tile.TileTypes.Hero
                        && x.OwnerId != myHero.ID)
                    {
                        //Dangerous heros nearby!
                        Hero h = game.LookupHero(x);
                        DirectionSet pathToThisHero = null;
                        heroPaths.TryGetValue(h, out pathToThisHero);
                        if (pathToThisHero != null 
                            && pathToThisHero.Distance <= 4
                            && h.Life > (myHero.Life - 10))
                        {
                            //Avoid!
                            totalCost += 10 * Math.Abs(pathToThisHero.Distance - 5);
                        }
                    }
                }

                return totalCost;
            });

            //Find our hero and important items
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

            //How safe should we be playing right now?
            int minimumHealth = 20;
            int nearestPlayerDistance = 99;
            int myOwnedMinesCount = game.LookupGoldMinesForHero(myHeroTile).Count();
            if (nearestNonPlayerHero != null)
            {
                nearestPlayerDistance = nearestNonPlayerHero.Distance;
            }

            //There's no need to play it *THAT* safe if there isn't somebody nearby...
            if (myOwnedMinesCount > 1
                && nearestPlayerDistance <= 4)
            {
                minimumHealth = 40;
            }
            else if (myOwnedMinesCount > 3
                     && nearestPlayerDistance <= 4)
            {
                //This will likely lead us to kiting back to taverns
                minimumHealth = 50;
            }

            //If we're not all the way healed, have the gold, and already by a tavern, let's just stay there and top off
            //Otherwise, if we're low health, go find the tavern and heal
            if (nearestTavern != null
                && (myHero.Life <= minimumHealth
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
                var ratio = goldMineRatios.OrderByDescending(x => x.Value).First();
                if (ratio.Value >= goldMineTargetRatio)
                {
                    Hero targetHero = null;

                    //Is this hero us?
                    if (ratio.Key.ID == myHero.ID)
                    {
                        //Is there a weaker hero nearby?
                        if (nearestNonPlayerHeroWithMines != null
                            && nearestNonPlayerHeroWithMines.Distance <= 4)
                        {
                            //Go get 'em, tiger!
                            targetHero = game.LookupHero(nearestNonPlayerHeroWithMines.TargetNode as Tile);
                            CoreHelpers.OutputLine("Taking down opportunity target, {0}! ({1}, {2})", targetHero.Name, nearestNonPlayerHeroWithMines.TargetNode.X, nearestNonPlayerHeroWithMines.TargetNode.Y);
                            return nearestNonPlayerHeroWithMines.Directions.FirstOrDefault();
                        }
                        else if (nearestTavern != null)
                        {
                            //Well camping is always nice thought!
                            CoreHelpers.OutputLine("Going to camp (tavern) because we are winning! ({0}, {1})", nearestTavern.TargetNode.X, nearestTavern.TargetNode.Y);
                            return nearestTavern.Directions.FirstOrDefault();
                        }
                    }

                    //Get the hero that we should gank
                    targetHero = ratio.Key;
                    var targetHeroPath = game.FindPathsToHeroes(myHeroTile, x => x.OwnerId == ratio.Key.ID, safeTravelFunction).FirstOrDefault();

                    //Are we healthy enough to do it?
                    if (targetHeroPath != null
                        && nearestUnownedGoldMine != null
                        && myHero.Life > 80
                        && (myHero.Life - 10) > targetHero.Life
                        && (targetHeroPath.Distance < (nearestUnownedGoldMine.Distance + 2)))
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
            CoreHelpers.OutputLine("Snoozing... (probably blocked!)");
            return Directions.Stay;
        }
    }
}
