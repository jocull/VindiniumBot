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

            //Find our hero
            var heroTile = gameState.FindMyHero();

            //If we're hurt, we should go heal.
            if (gameState.MyHero.Life <= 40
                && gameState.MyHero.Gold >= 2)
            {
                //Find the nearest tavern and go heal up
                var nearestTavern = gameState.Game.FindPathsToTaverns(heroTile, x => true).FirstOrDefault();

                Debug.WriteLine("Going for the nearest tavern! ({0}, {1})", nearestTavern.TargetNode.X, nearestTavern.TargetNode.Y);
                return nearestTavern.Directions.FirstOrDefault();
            }

            //Find the nearest unowned gold mine and try to capture it
            var nearestUnownedGoldMine = gameState.Game.FindPathsToGoldMines(heroTile, x => x.OwnerId != gameState.MyHero.ID)
                                                       .FirstOrDefault();

            Debug.WriteLine("Going for the nearest gold mine! ({0}, {1})", nearestUnownedGoldMine.TargetNode.X, nearestUnownedGoldMine.TargetNode.Y);
            return nearestUnownedGoldMine.Directions.FirstOrDefault();
        }
    }
}
