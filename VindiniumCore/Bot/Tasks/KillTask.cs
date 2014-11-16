using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VindiniumCore.GameTypes;

namespace VindiniumCore.Bot.Tasks
{
    public class KillTask : BotTask
    {
        public KillTask(int priority)
            : base(priority)
        {
        }

        protected override void _ResetInternal()
        {
        }

        protected override int _ScoreTaskInternal()
        {
            //Danger close?
            var pathsToEnemies = _H.Game.FindPathsToHeroes(_H.MyHeroTile, _H.UnownedTile);
            if (pathsToEnemies != null)
            {
                //Close players not standing next to taverns or their spawn points
                var closePlayers = pathsToEnemies.Where(x => x.Distance <= 4)
                                               .Where(p =>
                                               {
                                                   //Taverns
                                                   var hero = _H.Game.LookupHero(p.TargetNode as Tile);
                                                   var tavernPath = _H.Game.FindPathsToTaverns(p.TargetNode).FirstOrDefault();
                                                   if (tavernPath != null
                                                       && tavernPath.Distance <= 2
                                                       && hero.Life > 20) //Can't one shot them
                                                   {
                                                       return false;
                                                   }
                                                   return true;
                                               })
                                               .Where(p =>
                                               {
                                                   //Spawn points
                                                   var hero = _H.Game.LookupHero(p.TargetNode as Tile);
                                                   var spawnNode = _H.Board.GetNode(hero.Position.X, hero.Position.Y);
                                                   //_H.Board.GetNeighboringNodes(p.TargetNode, 1, true);
                                                   var spawnPath = _H.Game.FindPath(p.TargetNode, spawnNode);
                                                   if (spawnPath != null
                                                       && spawnPath.Distance <= 2)
                                                   {
                                                       return false;
                                                   }
                                                   return true;
                                               })
                                               .ToList();

                var superClosePlayers = closePlayers.Where(x => x.Distance <= 2);
                foreach (var p in superClosePlayers)
                {
                    var hero = _H.Game.LookupHero(p.TargetNode as Tile);
                    if (hero.Life <= 20
                        || hero.Life <= _H.MyHero.Life)
                    {
                        //We can kill this hero
                        //Will it lead to our death?
                        var otherHeroPathsFromTarget = _H.Game.FindPathsToHeroes(p.TargetNode, x => x.OwnerId != _H.MyHero.ID)
                                                              .Where(x => x.Distance <= 3);
                        if (otherHeroPathsFromTarget.Count() <= 1)
                        {
                            //Safe! Do it!
                            _LastBestPath = p;
                            _AnnouncementForHero("[Easy kill] Attempting kill super-close", hero);
                            return PRIORITY_HIGHEST;
                        }
                    }
                }

                foreach (var p in closePlayers)
                {
                    //Do they have any gold mines?
                    var hero = _H.Game.LookupHero(p.TargetNode as Tile);
                    var goldMines = _H.Game.LookupGoldMinesForHero(p.TargetNode as Tile);

                    if (hero.Life <= _H.MyHero.Life)
                    {
                        //Worth killing, if we can catch them!
                        _LastBestPath = p;
                        _AnnouncementForHero("[Easy kill] Attempting kill close", hero);
                        return PRIORITY_HIGH;
                    }
                }
            }

            _AnnouncementGeneral("[Easy kill] No priority");
            return PRIORITY_LOWEST; //Default - no consideration
        }
    }
}
