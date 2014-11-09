using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VindiniumCore.PathFinding
{
    /// <summary>
    /// Custom A* implementation
    /// http://www.policyalmanac.org/games/aStarTutorial.htm
    /// </summary>
    public class PathFinder
    {
        private INodeSet _NodeSet { get; set; }
        private HashSet<Node> _ClosedList { get; set; }
        private HashSet<Node> _OpenList { get; set; }
        private Dictionary<Node, NodePath> _NodePaths { get; set; }

        public PathFinder(INodeSet nodeSet)
        {
            _NodeSet = nodeSet;
            _ClosedList = new HashSet<Node>();
            _OpenList = new HashSet<Node>();
            _NodePaths = new Dictionary<Node, NodePath>();
        }

        private void _Reset(Node target)
        {
            _ClosedList.Clear();
            _OpenList.Clear();
            _NodePaths.Clear();

            //Pre-cache node path objects and costs
            foreach (var node in _NodeSet.GetAllNodes())
            {
                _NodePaths[node] = new NodePath(node, target);
            }
        }

        public NodePath FindShortestPath(Node start, Node target, int iterationLimit = 10000)
        {
            // Reset in case anything was present from another run
            _Reset(target);

            // 1) Add the starting node to the open list. 
            _OpenList.Add(start);
            _NodePaths[start].CostToThisPath = 0;

            // 2) Repeat the following...
            Node current = null;
            for (int iteration = 0; iteration < iterationLimit; iteration++)
            {
                // a) Look for the lowest F cost square on the open list.
                //    We refer to this as the current square.
                current = _OpenList.OrderBy(x => _NodePaths[x].TotalCost)
                                   .FirstOrDefault();

                if (current == null)
                {
                    //NO PATH COULD BE FOUND!
                    return null;
                }
                else if (_ClosedList.Contains(target))
                {
                    return _NodePaths[target];
                }

                NodePath currentPath = _NodePaths[current];

                // b) Switch it to the closed list.
                _OpenList.Remove(current);
                _ClosedList.Add(current);

                // c) For each of the 4 squares adjacent to this current square...
                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        if ((x == 0 && y == 0)
                            || (x != 0 && y != 0))
                        {
                            //Don't target the current node
                            //Don't allow diagonals
                            continue;
                        }

                        // -) If it is not walkable or if it is on the closed list, ignore it.
                        //    Otherwise do the following...
                        //    *** Note: I added special cases here to not block our start or target nodes
                        //              This is because you wouldn't move *through* them, but you can move *to* them.
                        Node adjacent = _NodeSet.GetRelativeNode(current, x, y);
                        if (adjacent != null
                            && (!adjacent.NodeBlocked || adjacent == start || adjacent == target)
                            && !_ClosedList.Contains(adjacent))
                        {
                            NodePath adjacentPath = _NodePaths[adjacent];

                            if (!_OpenList.Contains(adjacent))
                            {
                                // -) If it isn’t on the open list, add it to the open list.
                                //    Make the current square the parent of this square.
                                //    Record the F, G, and H costs of the square.
                                //    ***Note*** In this model, H doesn't change and F is automatic
                                _OpenList.Add(adjacent);
                                adjacentPath.ParentNodePath = currentPath;
                                adjacentPath.CostToThisPath = currentPath.CostToThisPath + adjacent.NodeMovementCost;
                            }
                            else
                            {
                                // -) If it is on the open list already, check to see if this
                                //    path to that square is better, using G cost as the measure.
                                if (adjacentPath.CostToThisPath < currentPath.CostToThisPath)
                                {
                                    // A lower G cost means that this is a better path.
                                    // If so, change the parent of the square to the current square,
                                    adjacentPath.ParentNodePath = currentPath;

                                    // and recalculate the G and F scores of the square. If you are
                                    // keeping your open list sorted by F score, you may need to resort
                                    // the list to account for the change.
                                }
                            }
                        }
                    }
                }
            }

            //No path could be found within reason
            return null;
        }
    }
}
