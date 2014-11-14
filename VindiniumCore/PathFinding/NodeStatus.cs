using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VindiniumCore.PathFinding
{
    public class NodeStatus
    {
        public int Cost { get; private set; }
        public bool Blocked { get; private set; }

        public NodeStatus(int cost, bool blocked)
        {
            this.Cost = cost;
            this.Blocked = blocked;
        }
    }
}
