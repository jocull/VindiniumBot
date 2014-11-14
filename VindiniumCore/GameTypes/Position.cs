using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VindiniumCore.PathFinding;

namespace VindiniumCore.GameTypes
{
    public class Position : IPosition
    {
        #region Core Properties

        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("X: {0}  Y: {1}", X, Y);
        }

        public override bool Equals(object obj)
        {
            Position p = obj as Position;
            if (p != null)
            {
                return p.X == this.X && p.Y == this.Y;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            //See:
            //http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode

            return new { X = X, Y = Y }.GetHashCode();
        }

        public static bool operator ==(Position a, Position b)
        {
            if ((object)a == null && (object)b == null)
            {
                return true;
            }
            else if ((object)a != null && (object)b != null)
            {
                return a.X == b.X && a.Y == b.Y;
            }
            return false;
        }

        public static bool operator !=(Position a, Position b)
        {
            return !(a == b);
        }
    }
}
