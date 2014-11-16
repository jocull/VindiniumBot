using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VindiniumCore.Bot.Tasks
{
    public class KillTask : EasyKillTask
    {
        public KillTask(int priority)
            : base (priority)
        {
        }

        protected override void _ResetInternal()
        {
        }

        protected override int _ScoreTaskInternal()
        {
            //Copy the easy kill task for now until we come up with an override
            _AnnouncementGeneral("[Kill] Defaulting to Easy Kill...");
            return base._ScoreTaskInternal();
        }
    }
}
