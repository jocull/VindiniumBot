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
        public Directions GetHeroMove(GameState gameState)
        {
            Debug.WriteLine("");
            Debug.WriteLine("It's now turn {0:#,0} of {1:#,0}", gameState.Game.Turn, gameState.Game.MaxTurns);
            Debug.WriteLine("The hero has {0:#,0} HP", gameState.MyHero.Life);
            Debug.WriteLine("The hero has {0:#,0} gold", gameState.MyHero.Gold);
            Debug.WriteLine("The hero is at {0}, {1}", gameState.MyHero.Position.X, gameState.MyHero.Position.Y);

            //Find our hero and important items
            var heroTile = gameState.FindMyHero();
            var nearestTavern = gameState.Game.FindPathsToTaverns(heroTile, x => true).FirstOrDefault();
            var nearestUnownedGoldMine = gameState.Game.FindPathsToGoldMines(heroTile, x => x.OwnerId != gameState.MyHero.ID).FirstOrDefault();
            var nearestNonPlayerHero = gameState.Game.FindPathsToHeroes(heroTile, x => x.OwnerId != gameState.MyHero.ID).FirstOrDefault();

            //If we're < 100, have the gold, and already by a tavern, let's just stay there and top off
            //Otherwise, if we're <= 40 health, go find the tavern and heal
            if (nearestTavern != null
                && (gameState.MyHero.Life <= 40
                    || (gameState.MyHero.Life < 95
                        && nearestTavern.Distance == 1))
                && gameState.MyHero.Gold >= 2)
            {
                Debug.WriteLine("Going for the nearest tavern! ({0}, {1})", nearestTavern.TargetNode.X, nearestTavern.TargetNode.Y);
                return nearestTavern.Directions.FirstOrDefault();
            }

            //Is there a weaker hero nearby that we can attack?
            if (nearestUnownedGoldMine != null
                && nearestNonPlayerHero != null
                && nearestNonPlayerHero.Distance < nearestUnownedGoldMine.Distance)
            {
                var hero = gameState.Game.LookupHero(nearestNonPlayerHero.TargetNode as Tile);
                if (hero != null
                    && hero.Life <= (gameState.MyHero.Life - 20))
                {
                    //Go for it
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
            return Directions.Stay;
        }
    }
}
