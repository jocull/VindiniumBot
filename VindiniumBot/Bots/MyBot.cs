using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VindiniumCore.Bot;
using VindiniumCore.GameTypes;

namespace VindiniumBot.Bots
{
    internal class MyBot : IRobot
    {
        public Directions GetHeroMove(GameState state)
        {
            Game game = state.Game;
            Hero myHero = state.MyHero;

            Debug.WriteLine("");
            Debug.WriteLine("It's now turn {0:#,0} of {1:#,0}", game.Turn, game.MaxTurns);
            Debug.WriteLine("The hero has {0:#,0} HP", myHero.Life);
            Debug.WriteLine("The hero has {0:#,0} gold", myHero.Gold);
            Debug.WriteLine("The hero is at {0}, {1}", myHero.Position.X, myHero.Position.Y);

            const double goldMineTargetRatio = 0.25d; //More than your fair share!

            //Find our hero and important items
            var myHeroTile = state.FindMyHero();
            var nearestTavern = game.FindPathsToTaverns(myHeroTile, x => true).FirstOrDefault();
            var nearestUnownedGoldMine = game.FindPathsToGoldMines(myHeroTile, x => x.OwnerId != myHero.ID).FirstOrDefault();
            var nearestNonPlayerHero = game.FindPathsToHeroes(myHeroTile, x => x.OwnerId != myHero.ID).FirstOrDefault();
            var goldMineRatios = game.LookupGoldMineRatiosForHeros();

            //If we're < 100, have the gold, and already by a tavern, let's just stay there and top off
            //Otherwise, if we're <= 40 health, go find the tavern and heal
            if (nearestTavern != null
                && (myHero.Life <= 25
                    || (myHero.Life < 95
                        && nearestTavern.Distance == 1))
                && myHero.Gold >= 2)
            {
                Debug.WriteLine("Going for the nearest tavern! ({0}, {1})", nearestTavern.TargetNode.X, nearestTavern.TargetNode.Y);
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
                    var targetHeroPath = game.FindPathsToHeroes(myHeroTile, x => x.OwnerId == ratio.Key.ID).FirstOrDefault();

                    //Are we healthy enough to do it?
                    if (targetHeroPath != null
                        && nearestUnownedGoldMine != null
                        && myHero.Life > 80
                        && (myHero.Life - 20) > targetHero.Life
                        && (targetHeroPath.Distance < nearestUnownedGoldMine.Distance))
                    {
                        Debug.WriteLine("Going to kill {0} for the $$$! ({1}, {2})", targetHero.Name, targetHeroPath.TargetNode.X, targetHeroPath.TargetNode.Y);
                        return targetHeroPath.Directions.FirstOrDefault();
                    }
                }
            }

            //Is there a weaker hero nearby that we can attack?
            if (nearestUnownedGoldMine != null
                && nearestNonPlayerHero != null
                && nearestNonPlayerHero.Distance < nearestUnownedGoldMine.Distance)
            {
                var hero = game.LookupHero(nearestNonPlayerHero.TargetNode as Tile);
                if (hero != null
                    && hero.Life <= (myHero.Life - 20))
                {
                    //Go for it
                    Debug.WriteLine("Going to pick off {0}! ({1}, {2})", hero.Name, hero.Position.X, hero.Position.Y);
                    return nearestNonPlayerHero.Directions.FirstOrDefault();
                }
            }

            //Find the nearest unowned gold mine and try to capture it
            if (nearestUnownedGoldMine != null)
            {
                Debug.WriteLine("Going for the nearest gold mine! ({0}, {1})", nearestUnownedGoldMine.TargetNode.X, nearestUnownedGoldMine.TargetNode.Y);
                return nearestUnownedGoldMine.Directions.FirstOrDefault();
            }

            //Don't go anywhere if there's truly nothing to do...
            Debug.WriteLine("Snoozing...");
            return Directions.Stay;
        }
    }
}
