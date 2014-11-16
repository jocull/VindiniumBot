using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VindiniumCore.GameTypes;

namespace VindiniumCore.Bot.Tasks
{
    public class CampingTask : BotTask
    {
        private bool Stay { get; set; }

        public CampingTask(int priority)
            : base(priority)
        {
        }

        protected override void _ResetInternal()
        {
            Stay = false;
        }

        protected override int _ScoreTaskInternal()
        {
            //Are we winning?
            if (_H.WillWinGame
                && _H.MyHero.Gold >= _H.MostGoldInGame) //Safety threshold
            {
                //Enemy nearby?
                var enemyPath = _H.Game.FindPathsToHeroes(_H.MyHeroTile, _H.UnownedTile)
                                        .Where(p => p.Distance <= 4)
                                        .FirstOrDefault();

                if (enemyPath != null)
                {
                    var hero = _H.Game.LookupHero(enemyPath.TargetNode as Tile);
                    var goldMines = _H.Game.LookupGoldMinesForHero(enemyPath.TargetNode as Tile);
                    if (_H.MyHero.Life > hero.Life
                        && goldMines.Any())
                    {
                        //Pick them off
                        _LastBestPath = enemyPath;
                        _AnnouncementForHero("[Camping] Picking off nearby hero", hero);
                        return PRIORITY_HIGH;
                    }
                }

                //Is there a mine we don't own really close by?
                var goldMinePath = _BestUnownedGoldMinePaths()
                                            .Where(x => x.Distance <= 4)
                                            .FirstOrDefault();
                if (goldMinePath != null)
                {
                    _LastBestPath = goldMinePath;
                    _AnnouncementForGoldMine("[Camping] Grabbing nearby mine", goldMinePath.TargetNode as Tile);
                    return PRIORITY_HIGH;
                }

                var tavernPath = _BestTavernPaths().FirstOrDefault();
                if (tavernPath != null)
                {
                    if (tavernPath.Distance <= 1
                        && _H.MyHero.Life >= 80)
                    {
                        Stay = true;
                        _AnnouncementForTavern("[Camping] Waiting by tavern", tavernPath.TargetNode as Tile);
                    }
                    else
                    {
                        _LastBestPath = tavernPath;
                        _AnnouncementForTavern("[Camping] Heading to tavern", tavernPath.TargetNode as Tile);
                    }
                    return PRIORITY_HIGH;
                }
            }

            //Don't camp now!
            _AnnouncementGeneral("[Camping] No priority");
            return PRIORITY_LOWEST;
        }

        public override bool CanPerformTask
        {
            get
            {
                if (Stay)
                {
                    return true;
                }

                return base.CanPerformTask;
            }
        }

        public override Directions? PerformTask()
        {
            if (Stay)
            {
                return Directions.Stay;
            }

            return base.PerformTask();
        }
    }
}
