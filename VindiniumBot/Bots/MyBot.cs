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

            return Directions.Stay;
        }
    }
}
