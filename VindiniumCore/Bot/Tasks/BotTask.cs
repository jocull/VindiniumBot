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
