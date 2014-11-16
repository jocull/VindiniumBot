﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VindiniumCore.GameTypes;

namespace VindiniumCore.Bot.Tasks
{
    public class StuckTask : BotTask
    {
        public StuckTask(int priority)
            : base(priority)
        {
        }

        protected override void _ResetInternal()
        {
        }

        protected bool _ShouldRunAway(out int score, out string details)
        {
            //Nearest tavern & enemy
            var nearestTavernPath = _H.Game.FindPathsToTaverns(_H.MyHeroTile).FirstOrDefault();
            var nearestEnemyPath = _H.Game.FindPathsToHeroes(_H.MyHeroTile, _H.UnownedTile).FirstOrDefault();

            //Consider our spawn...
            var mySpawnPoint = _H.Game.FindSpawnPoints(x => x.ID == _H.MyHero.ID).First();
            var mySpawnPointPath = _H.Game.FindPath(_H.MyHeroTile, mySpawnPoint);
            if (mySpawnPointPath != null
                && mySpawnPointPath.Distance <= 4)
            {
                //I'm near my spawn
                //Is the nearest enemy sitting on a tavern?
                if (nearestEnemyPath != null
                    && nearestTavernPath != null
                    && nearestEnemyPath.Distance <= 4)
                {
                    var enemyTavernPath = _H.Game.FindPath(nearestEnemyPath.TargetNode, nearestTavernPath.TargetNode);
                    if (enemyTavernPath != null
                        && enemyTavernPath.Distance <= 1)
                    {
                        //Avoid. We we'll just get stuck respawning
                        score = PRIORITY_HIGH;
                        details = "avoiding my spawn-lock";
                        return true;
                    }
                }
            }

            //Consider enemy spawns...
            var enemySpawnPoints = _H.Game.FindSpawnPoints(x => x.ID != _H.MyHero.ID);
            var enemySpawnPointClosest = _H.Game.FindPaths(_H.MyHeroTile, enemySpawnPoints).FirstOrDefault();
            if (enemySpawnPointClosest != null
                && enemySpawnPointClosest.Distance <= 3)
            {
                //I'm near an enemy spawn
                if (nearestEnemyPath != null
                    && nearestEnemyPath.Distance <= 2)
                {
                    //We won't win this fight. Just run away...
                    score = PRIORITY_HIGH;
                    details = "avoiding enemy spawn-lock";
                    return true;
                }
            }

            //Consider joined taverns and heroes
            // If I'm by a tavern
            //      and you're a by a tavern then....
            //      and I'm by you...
            if (nearestTavernPath != null
                && nearestTavernPath.Distance <= 1)
            {
                var myPathToEnemy = _H.Game.FindPath(_H.MyHeroTile, nearestEnemyPath.TargetNode);
                var enemysPathToTavern = _H.Game.FindPathsToTaverns(nearestEnemyPath.TargetNode).FirstOrDefault();
                if (myPathToEnemy != null
                    && enemysPathToTavern != null
                    && myPathToEnemy.Distance <= 1
                    && enemysPathToTavern.Distance <= 1)
                {
                    score = PRIORITY_HIGHEST;
                    details = "avoiding tavern-lock";
                    return true;
                }
            }

            //Consider joined taverns/spawns and gold mines...
            //  TODO: Yeah, maybe another day when I'm more motivated...

            score = PRIORITY_LOWEST;
            details = "";
            return false;
        }

        protected override int _ScoreTaskInternal()
        {
            //Did we decide to run away anywhere?
            int score;
            string details;
            if (_ShouldRunAway(out score, out details))
            {
                _LastBestPath = _BestUnownedGoldMinePaths().FirstOrDefault();
                if (_LastBestPath != null)
                {
                    _AnnouncementForGoldMine("[Stuck] Getting unstuck (" + details + ")", _LastBestPath.TargetNode as Tile);
                }
                else
                {
                    _AnnouncementGeneral("[Stuck] No exit path found! (" + details + ")");
                }
                return score;
            }

            _AnnouncementGeneral("[Stuck] No priority (" + details + ")");
            return PRIORITY_LOWEST;
        }
    }
}