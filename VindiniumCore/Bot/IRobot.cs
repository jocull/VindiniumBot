using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VindiniumCore.GameTypes;

namespace VindiniumCore.Bot
{
    /// <summary>
    /// I did it for the puns.
    /// </summary>
    public interface IRobot
    {
        void Reset();
        Directions GetHeroMove(GameState gameState);
    }
}
