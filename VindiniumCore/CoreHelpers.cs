using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VindiniumCore
{
    public static class CoreHelpers
    {
        public static void OutputLine(string value)
        {
            Debug.WriteLine(value);
            Console.WriteLine(value);
        }

        public static void OutputLine(string format, params object[] args)
        {
            string output = string.Format(format, args);
            Debug.WriteLine(output);
            Console.WriteLine(output);
        }
    }
}
