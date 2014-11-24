using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VindiniumCore.PathFinding
{
    public class NodePath
    {
        public Node SourceNode { get; set; }
        public NodePath ParentNodePath { get; set; }

        /// <summary>
        /// Sometimes called `H`.
        /// This value is pre-calculated and cached from the original target node.
        /// </summary>
        public int Heuristic { get; private set; }

        /// <summary>
        /// Sometimes called `G`.
        /// </summary>
        public int CostToThisPath { get; set; }

        /// <summary>
        /// Sometimes called `F`, where F = G + H
        /// </summary>
        public int TotalCost
        {
            get
            {
                return CostToThisPath + Heuristic;
            }
        }

        public NodePath(Node source)
        {
            this.SourceNode = source;
            this.Heuristic = 0; //Defaults
            this.CostToThisPath = 0;
        }

        public void ResetPath(Node target)
        {
            this.ParentNodePath = null;
            this.Heuristic = this.SourceNode.NodeHeuristic(target);
        }

        public IEnumerable<NodePath> ParentNodePaths
        {
            get
            {
                NodePath path = this;
                while (path != null)
                {
                    yield return path;
                    path = path.ParentNodePath;
                }
            }
        }
    }
}
