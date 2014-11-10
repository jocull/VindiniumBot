using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VindiniumCore.PathFinding
{
    public interface INodeSet
    {
        Node GetNode(int x, int y);
        Node GetRelativeNode(Node baseNode, int x, int y);
        IEnumerable<Node> GetNeighboringNodes(Node baseNode, int span = 1, bool diagonals = false);
        IEnumerable<Node> GetAllNodes();
    }
}
