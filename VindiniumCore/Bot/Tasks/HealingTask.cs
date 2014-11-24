using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VindiniumCore.GameTypes;

namespace VindiniumCore.Bot.Tasks
{
    public class HealingTask : BotTask
    {
        public HealingTask(int priority)
            : base(priority)
        {
        }

        protected override void _ResetInternal()
        {
        }

        protected override int _ScoreTaskInternal()
        {
            _LastBestPath = _H.BestTavernPath;

            if (_LastBestPath != null
                && _H.MyHero.Gold >= 2)
            {
                if (_LastBestPath.Distance <= 1
                    && _H.MyHero.Life < 90)
                {
                    //Top off while by tavern
                    _AnnouncementForTavern("[Heal] Remaining by tavern", _LastBestPath.TargetNode as Tile);
                    return PRIORITY_HIGHEST;
                }
                else if (_H.MyHero.Life <= 20)
                {
                    //Not going to do much good in this state
                    _AnnouncementForTavern("[Heal] Running for closest tavern (weak!)", _LastBestPath.TargetNode as Tile);
                    return PRIORITY_HIGH;
                }

                int ownedMineCount = _H.GoldMinesOwned.Count();
                int totalMineCount = _H.Game.FindGoldMines().Count();
                double ownedMineRatio = (double)ownedMineCount / (double)totalMineCount;
                if (ownedMineRatio >= 0.25)
                {
                    //Is there danger nearby?
                    var enemyPaths = _H.Game.FindPathsToHeroes(_H.MyHeroTile, x => x.OwnerId != _H.MyHero.ID)
                                            .Where(x => x.Distance <= 4);

                    if (enemyPaths.Any()
                        && _H.MyHero.Life <= 50)
                    {
                        //Healing is a good idea, due to close danger
                        _AnnouncementForTavern("[Heal] Running for closest tavern (danger!)", _LastBestPath.TargetNode as Tile);
                        return PRIORITY_NORMAL;
                    }
                }
            }
            
            _AnnouncementGeneral("[Heal] No priority");
            return PRIORITY_LOWEST;
        }
    }
}
