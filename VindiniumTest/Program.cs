using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VindiniumCore.GameTypes;

namespace VindiniumTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Board board = new Board
            {
                Size = 18,
                TilesAsString = "##############        ############################        ##############################    ##############################$4    $4############################  @4    ########################  @1##    ##    ####################  []        []  ##################        ####        ####################  $4####$4  ########################  $4####$4  ####################        ####        ##################  []        []  ####################  @2##    ##@3  ########################        ############################$-    $-##############################    ##############################        ############################        ##############",
                Finished = false
            };

            Console.WriteLine(board.ToString());
            Console.ReadLine();
        }
    }
}
