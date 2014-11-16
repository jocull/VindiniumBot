using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VindiniumCore.GameTypes;

namespace VindiniumCore.Bot.Tasks
{
    public class CaptureMineTask : BotTask
    {
        public CaptureMineTask(int priority)
            : base (priority)
        {
        }

        protected override void _ResetInternal()
        {
        }

        protected override int _ScoreTaskInternal()
        {
            _LastBestPath = _BestUnownedGoldMinePaths().FirstOrDefault();

            if (_LastBestPath != null)
            {
                _AnnouncementForGoldMine("[Capture Mine] Going for mine", _LastBestPath.TargetNode as Tile);
                return PRIORITY_NORMAL;
            }

            _AnnouncementGeneral("[Capture Mine] No priority");
            return PRIORITY_LOWEST;
        }
    }
}
