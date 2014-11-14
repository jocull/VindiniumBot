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

            const double goldMineTargetRatio = 0.255d; //More than your fair share!

            //Precalculate paths to other heros
            Tile myHeroTile = state.FindMyHero();
            int mostPlayerGold = game.Heroes.Max(x => x.Gold);

            //How safe should we be traveling right now?
            var safeTravelFunction = new Func<Node, NodeStatus>(node =>
            {
                Tile t = node as Tile;
                var neighbors = state.Game.Board.GetNeighboringNodes(t, 1, true).Select(x => x as Tile);

                foreach (var x in neighbors)
                {
                    //Any heros in the area?
                    if (x.TileType == Tile.TileTypes.Hero
                        && x.OwnerId != myHero.ID)
                    {
                        //Dangerous heros in the way?
                        Hero h = game.LookupHero(x);
                        if (h.Life > myHero.Life)
                        {
                            //Avoid!
                            return new NodeStatus(30, true);
                        }
                    }
                }
                return new NodeStatus(1, false);
            });

            //Where are the nearest safe taverns?
            var nearestTavern = game.FindPathsToTaverns(myHeroTile, statusFunc: safeTravelFunction).FirstOrDefault(); //The safe travel function here should avoid confrontations

            //Where's the nearest non-player hero?
            var nearestNonPlayerHero = game.FindPathsToHeroes(myHeroTile, x =>
            {
                if (x.OwnerId != myHero.ID)
                {
                    return true;
                }
                return false;
            }, safeTravelFunction).FirstOrDefault();

            //How about the nearest non-player hero with mines?
            var nearestNonPlayerHeroWithMines = game.FindPathsToHeroes(myHeroTile, x =>
            {
                if (x.OwnerId != myHero.ID
                    && game.LookupGoldMinesForHero(x).Any())
                {
                    //Don't consider heroes that are sitting by a tavern
                    var adjacentNodes = game.Board.GetNeighboringNodes(x, 1, false).Select(n => n as Tile);
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
            var nearestUnownedGoldMine = game.FindPathsToGoldMines(myHeroTile, x =>
            {
                //Block mines we own
                if (x.OwnerId != myHero.ID)
                {
                    //We'll get stuck if in this situation...
                    //          $-@2
                    //          @1[]
                    //Block mines that will get us stuck
                    var adjacentNodes = game.Board.GetNeighboringNodes(x, 1, true).Select(n => n as Tile);
                    if (adjacentNodes.Any(t => t.TileType == Tile.TileTypes.Hero && t.OwnerId != myHero.ID)) //enemy hero
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
            Hero nearestPlayer = null;
            int myOwnedMinesCount = game.LookupGoldMinesForHero(myHeroTile).Count();
            if (nearestNonPlayerHero != null)
            {
                nearestPlayer = game.LookupHero(nearestNonPlayerHero.TargetNode as Tile);
                nearestPlayerDistance = nearestNonPlayerHero.Distance;
            }

            //There's no need to play it *THAT* safe if there isn't somebody nearby...
            if (myOwnedMinesCount > 1
                && nearestPlayerDistance <= 4
                && nearestPlayer != null
                && nearestPlayer.Life >= myHero.Life)
                
            {
                minimumHealth = 40;
            }
            else if (myOwnedMinesCount > 3
                     && nearestPlayerDistance <= 4
                     && nearestPlayer != null
                     && nearestPlayer.Life >= myHero.Life)
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
                    //Is this hero us?
                    if (ratio.Key.ID == myHero.ID)
                    {
                        //Is there a weaker hero nearby?
                        if (nearestNonPlayerHeroWithMines != null
                            && nearestNonPlayerHeroWithMines.Distance <= 4)
                        {
                            //Go get 'em, tiger!
                            Hero targetHero = game.LookupHero(nearestNonPlayerHeroWithMines.TargetNode as Tile);
                            if (targetHero.Life < myHero.Life)
                            {
                                CoreHelpers.OutputLine("Taking down opportunity target, {0}! ({1}, {2})", targetHero.Name, nearestNonPlayerHeroWithMines.TargetNode.X, nearestNonPlayerHeroWithMines.TargetNode.Y);
                                return nearestNonPlayerHeroWithMines.Directions.FirstOrDefault();
                            }
                        }
                        if (nearestTavern != null
                            && myHero.Gold >= mostPlayerGold)
                        {
                            //Is there another gold mine nearby we can get easily?
                            if (nearestTavern.Distance <= 3
                                && nearestUnownedGoldMine != null
                                && nearestUnownedGoldMine.Distance <= 4 //Extra space so we'll capture it by moving "into it"...
                                && myHero.Life >= 60)
                            {
                                CoreHelpers.OutputLine("Going to snag gold mine near camp site! ({0}, {1})", nearestUnownedGoldMine.TargetNode.X, nearestUnownedGoldMine.TargetNode.Y);
                                return nearestUnownedGoldMine.Directions.FirstOrDefault();
                            }

                            //Well camping is always nice thought!
                            CoreHelpers.OutputLine("Going to camp (tavern) because we are winning! ({0}, {1})", nearestTavern.TargetNode.X, nearestTavern.TargetNode.Y);
                            if (nearestTavern.Distance == 1
                                && myHero.Life > 50)
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
                        var targetHeroTile = game.FindTiles(x => x.TileType == Tile.TileTypes.Hero && x.OwnerId == targetHero.ID).FirstOrDefault();
                        var targetHeroPath = game.FindPathsToHeroes(myHeroTile, x => x.OwnerId == ratio.Key.ID, safeTravelFunction).FirstOrDefault();
                        var targetHeroPathToTavern = game.FindPathsToTaverns(targetHeroTile, x => true).FirstOrDefault();

                        //Are we healthy enough to do it?
                        if (targetHeroPath != null
                            && nearestUnownedGoldMine != null
                            && myHero.Life >= 50
                            && targetHero.Life < myHero.Life
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
                var hero = game.LookupHero(nearestNonPlayerHeroWithMines.TargetNode as Tile);
                if (hero != null
                    && hero.Life < myHero.Life)
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
