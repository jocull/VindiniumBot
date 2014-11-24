using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VindiniumCore.GameTypes;

namespace VindiniumCore.Bot.Tasks
{
    public class FleeTask : BotTask
    {
        public FleeTask(int priority)
            : base(priority)
        {
        }

        protected override void _ResetInternal()
        {
        }

        protected override int _ScoreTaskInternal()
        {
            //Find the nearest unblocked tavern and get there safely!!!
            _LastBestPath = _H.BestTavernPath;

            //Danger close?
            var closestEnemy = _H.Game.FindPathsToHeroes(_H.MyHeroTile, _H.UnownedTile).FirstOrDefault();
            if (closestEnemy != null)
            {
                //Are they close enough to worry about?
                if (closestEnemy.Distance <= 3)
                {
                    //Do they have more health than us?
                    var hero = _H.Game.LookupHero(closestEnemy.TargetNode as Tile);
                    if (hero.Life > _H.MyHero.Life)
                    {
                        //Run!
                        _AnnouncementForHero("[Flee] Running from enemy", hero);
                        return PRIORITY_HIGHEST;
                    }
                }
            }

            _AnnouncementGeneral("[Flee] No priority");
            return PRIORITY_LOWEST; //Default - no consideration
        }
    }
}
