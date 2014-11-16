using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VindiniumCore;
using VindiniumCore.Bot;
using VindiniumCore.Bot.Tasks;
using VindiniumCore.GameTypes;
using VindiniumCore.PathFinding;

namespace VindiniumBot.Bots
{
    internal class MyBot : IRobot
    {
        #region Properties

        private GameStateHelper _H { get; set; }
        private BotTask[] _Tasks { get; set; }

        #endregion

        public MyBot()
        {
            Reset();

            // Task orders:
            // =================================
            // Get out of a stuck situation
            // Claims easy kills we can definitely win
            // Flee a dangerous position
            // Heal or continue healing
            // Camping (by winning distance)
            // *** (Copy, easy kill) Kill a player (immenent death / enemy wounded / gold mines (ratio?) / distance)
            // Capture a gold mine (by distance / life)
            _Tasks = new BotTask[]
            {
                new StuckTask(1),
                new EasyKillTask(2),
                new FleeTask(3),
                new HealingTask(4),
                new CampingTask(5),
                //new KillTask(6),
                new CaptureMineTask(7),
            };
        }

        public void Reset()
        {
            //Release all references, clear lists, dictionaries, etc...
            _H = null;
        }

        private void _Init(GameState state)
        {
            //Pre-calculate and cache important properties
            _H = new GameStateHelper(state);
        }

        public Directions GetHeroMove(GameState state)
        {
            _Init(state);
            return _FindMove();
        }

        private IEnumerable<BotTask> _FindTasks()
        {
            foreach (var t in _Tasks)
            {
                t.Reset();
                t.ScoreTask(_H);
            }

            return _Tasks.Where(x => x.CanPerformTask)
                         .OrderBy(x => x.Score)
                         .ThenBy(x => x.Priority);
        }

        private Directions _FindMove()
        {
            CoreHelpers.OutputLine("");
            CoreHelpers.OutputLine("It's now turn {0:#,0} of {1:#,0}", _H.Game.Turn, _H.Game.MaxTurns);
            CoreHelpers.OutputLine("The hero has {0:#,0} HP", _H.MyHero.Life);
            CoreHelpers.OutputLine("The hero has {0:#,0} gold", _H.MyHero.Gold);
            CoreHelpers.OutputLine("The hero is at {0}, {1}", _H.MyHero.Position.X, _H.MyHero.Position.Y);

            List<BotTask> tasks = _FindTasks().ToList();
            CoreHelpers.OutputLine("");
            CoreHelpers.OutputLine("Found Tasks");
            CoreHelpers.OutputLine("===============");
            foreach (var task in tasks)
            {
                CoreHelpers.OutputLine(string.Format("{0} | Score: {1}, Priority: {2}", task.Announcement, task.Score, task.Priority));
            }
            CoreHelpers.OutputLine("");

            BotTask bestTask = tasks.FirstOrDefault();
            if (bestTask != null)
            {
                Directions? taskResult = bestTask.PerformTask();
                if (taskResult.HasValue)
                {
                    if (!string.IsNullOrEmpty(bestTask.Announcement))
                    {
                        CoreHelpers.OutputLine(bestTask.Announcement);
                    }
                    return taskResult.Value;
                }
            }

            //Don't go anywhere if there's truly nothing to do...
            CoreHelpers.OutputLine("Snoozing... (probably blocked!)");
            return Directions.Stay;
        }
    }
}
