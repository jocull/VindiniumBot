using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VindiniumCore.GameTypes;

namespace VindiniumCore.PathFinding
{
    public abstract class Node : IPosition, IComparable
    {
        public abstract int NodeMovementCost { get; }
        public abstract bool NodeBlocked { get; }

        /// <summary>
        /// Sometimes called `H`
        /// </summary>
        public int NodeHeuristic(Node target)
        {
            //Return the Manhattan distance...
            return 10 * Math.Abs(this.X - target.X) + Math.Abs(this.Y - target.Y);
        }

        #region IPosition implementation

        public int X { get; set; }
        public int Y { get; set; }

        #endregion

        #region IComparable implementation

        public int CompareTo(object obj)
        {
            Node other = obj as Node;
            if (other != null)
            {
                return NodeMovementCost.CompareTo(other.NodeMovementCost);
            }
            return 0;
        }

        #endregion
    }
}
