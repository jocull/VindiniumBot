using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VindiniumCore.GameTypes;

namespace VindiniumCore.Bot.Tasks
{
    public abstract class BotTask
    {
        #region Constants

        public const int PRIORITY_HIGHEST = 0;
        public const int PRIORITY_HIGH = 25;
        public const int PRIORITY_NORMAL = 50;
        public const int PRIORITY_LOW = 75;
        public const int PRIORITY_LOWEST = 100;

        #endregion

        /// <summary>
        /// Lower numbers will be weighted first
        /// </summary>
        public int Priority { get; private set; }

        /// <summary>
        /// The lower the score, the higher the priority
        /// </summary>
        public int Score { get; protected set; }

        /// <summary>
        /// Information about the decision made
        /// </summary>
        public string Announcement { get; protected set; }

        /// <summary>
        /// Checks if the task can actually be performed
        /// </summary>
        public virtual bool CanPerformTask
        {
            get
            {
                return _LastBestPath != null;
            }
        }

        protected BotTask(int priority)
        {
            Priority = priority;
        }

        protected GameStateHelper _H { get; set; }
        protected DirectionSet _LastBestPath { get; set; }

        public void Reset()
        {
            Score = 0;

            _H = null;
            _LastBestPath = null;
            Announcement = null;
            _ResetInternal();
        }

        public void ScoreTask(GameStateHelper stateHelper)
        {
            _H = stateHelper;
            Score = _ScoreTaskInternal();
        }

        public virtual Directions? PerformTask()
        {
            if (_LastBestPath != null)
            {
                return _LastBestPath.Directions.FirstOrDefault();
            }

            //No path found
            return null;
        }

        protected abstract void _ResetInternal();
        protected abstract int _ScoreTaskInternal();

        #region Helper methods

        protected IEnumerable<DirectionSet> _BestUnownedGoldMinePaths(IEnumerable<Tile> excludedGoldMinesOrTiles = null)
        {
            if (excludedGoldMinesOrTiles == null)
            {
                //Exclude all gold mines that are by another hero
                excludedGoldMinesOrTiles = _H.Game.FindGoldMines(_H.UnownedTile)
                                                  .Where(t =>
                                                  {
                                                      var paths = _H.Game.FindPathsToHeroes(t, _H.UnownedTile)
                                                                         .Where(p => p.Distance <= 1);
                                                      if (paths.Any())
                                                      {
                                                          //This is an excluded mine
                                                          return true;
                                                      }
                                                      //This mine is safe
                                                      return false;
                                                  })
                                                  .ToList();
            }

            var safestGoldMinePaths = _H.Game.FindPathsToGoldMines(_H.MyHeroTile, _H.UnownedTile, _H.TravelModeBlockingHeroes);
            var saferGoldMinePaths = _H.Game.FindPathsToGoldMines(_H.MyHeroTile, _H.UnownedTile, _H.TravelModeAvoidingHeroes);
            var straightGoldMinePaths = _H.Game.FindPathsToGoldMines(_H.MyHeroTile, _H.UnownedTile);
            var allPaths = safestGoldMinePaths.Union(saferGoldMinePaths)
                                              .Union(straightGoldMinePaths)
                                              .Where(x => !excludedGoldMinesOrTiles.Contains(x.TargetNode));

            return allPaths;
        }

        protected IEnumerable<DirectionSet> _BestTavernPaths(IEnumerable<Tile> excludedTavernsOrTiles = null)
        {
            if (excludedTavernsOrTiles == null)
            {
                excludedTavernsOrTiles = Enumerable.Empty<Tile>();
            }

            var safestPathsToTaverns = _H.Game.FindPathsToTaverns(_H.MyHeroTile, statusFunc: _H.TravelModeBlockingHeroes);
            var safePathsToTaverns = _H.Game.FindPathsToTaverns(_H.MyHeroTile, statusFunc: _H.TravelModeAvoidingHeroes);
            var straightPathsToTaverns = _H.Game.FindPathsToTaverns(_H.MyHeroTile);
            var allPaths = safestPathsToTaverns.Union(safePathsToTaverns)
                                               .Union(straightPathsToTaverns)
                                               .Where(x => !excludedTavernsOrTiles.Contains(x.TargetNode));

            return allPaths;
        }

        protected void _AnnouncementGeneral(string info)
        {
            Announcement = info;
        }

        protected void _AnnouncementForHero(string info, Hero hero)
        {

            Announcement = string.Format("{0}. [{1}] - {2}, {3}", info,
                                                        hero.Name,
                                                        hero.Position.X,
                                                        hero.Position.Y);
        }

        protected void _AnnouncementForGoldMine(string info, Tile mine)
        {
            Announcement = string.Format("{0}. [Gold mine] {1}, {2}", info, mine.X, mine.Y);
        }

        protected void _AnnouncementForTavern(string info, Tile tavern)
        {
            Announcement = string.Format("{0}. [Tavern] {1}, {2}", info, tavern.X, tavern.Y);
        }

        #endregion
    }
}
