using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VindiniumCore.PathFinding;

namespace VindiniumCore.GameTypes
{
    public class DirectionSet
    {
        public Node SourceNode { get; private set; }
        public Node TargetNode { get; private set; }
        public IEnumerable<Node> NodeSequence { get; private set; }
        public IEnumerable<Directions> Directions { get; private set; }
        
        public int Distance
        {
            get { return Directions.Count(); }
        }

        public DirectionSet(NodePath targetNodePath)
        {
            this.TargetNode = targetNodePath.SourceNode;
            this.SourceNode = targetNodePath.ParentNodePaths.Last().SourceNode;

            //Calculate the directions we will need to take
            var fullNodeSequence = targetNodePath.ParentNodePaths
                                                 .Reverse()
                                                 .Select(x => x.SourceNode);

            //Don't include our starting point
            this.NodeSequence = fullNodeSequence.Skip(1).ToList();

            //Calculate all the steps our hero would need to take
            List<Directions> directions = new List<GameTypes.Directions>();
            Node lastNode = fullNodeSequence.FirstOrDefault();
            foreach (var node in fullNodeSequence)
            {
                if (lastNode != null)
                {
                    if (lastNode.X < node.X)
                    {
                        directions.Add(GameTypes.Directions.East);
                    }
                    else if (lastNode.X > node.X)
                    {
                        directions.Add(GameTypes.Directions.West);
                    }
                    else if (lastNode.Y < node.Y)
                    {
                        directions.Add(GameTypes.Directions.South);
                    }
                    else if (lastNode.Y > node.Y)
                    {
                        directions.Add(GameTypes.Directions.North);
                    }
                }
                lastNode = node;
            }
            this.Directions = directions;
        }
    }
}
