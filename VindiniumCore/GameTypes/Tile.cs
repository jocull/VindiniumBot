using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VindiniumCore.PathFinding;

namespace VindiniumCore.GameTypes
{
    public class Tile : Node
    {
        public enum TileTypes
        {
            Unknown = '?',
            Open = ' ',
            ImpassableWood = '#',
            Hero = '@',
            Tavern = '[',
            GoldMine = '$'
        }

        #region Core Properties

        public TileTypes TileType { get; set; }
        public byte? OwnerId { get; set; }

        #endregion

        #region Map parsing and serialization

        /*  Example maps from documentation:
            +----------------------------------------+
            |######$-    $-############$-    $-######|
            |######        ##        ##        ######|
            |####[]    ####            ####    []####|
            |##      ####  ##        ##  ####      ##|
            |####            $-    $-            ####|
            |##########  @1            @4  ##########|
            |############  ####    ####  ############|
            |$-##$-        ############        $-##$-|
            |  $-      $-################$-      $-  |
            |        ########################        |
            |        ########################        |
            |  $-      $-################$-      $-  |
            |$-##$-        ############        $-##$-|
            |############  ####    ####  ############|
            |##########  @2            @3  ##########|
            |####            $-    $-            ####|
            |##      ####  ##        ##  ####      ##|
            |####[]    ####            ####    []####|
            |######        ##        ##        ######|
            |######$-    $-############$-    $-######|
            +----------------------------------------+

            ====> Input (one long string)

            ##############        ############################        ##############################    
            ##############################$4    $4############################  @4    ########################  @1##    ##    
            ####################  []        []  ##################        ####        ####################  $4####$4  
            ########################  $4####$4  ####################        ####        ##################  []        []  
            ####################  @2##    ##@3  ########################        ############################$-    
            $-##############################    ##############################        ############################        
            ##############

            ====> Rebuilt Output (split by size)

               ##############        ##############
               ##############        ##############
               ################    ################
               ##############$4    $4##############
               ##############  @4    ##############
               ##########  @1##    ##    ##########
               ##########  []        []  ##########
               ########        ####        ########
               ############  $4####$4  ############
               ############  $4####$4  ############
               ########        ####        ########
               ##########  []        []  ##########
               ##########  @2##    ##@3  ##########
               ##############        ##############
               ##############$-    $-##############
               ################    ################
               ##############        ##############
               ##############        ##############
        */
        public static Tile[][] ParseTileChars(int size, string tileChars)
        {
            //Allocate
            Tile[][] result = new Tile[size][];
            for (int i = 0; i < size; i++)
            {
                result[i] = new Tile[size];
            }

            //Parse
            for (int i = 0; i < tileChars.Length; i += 2)
            {
                Tile t = new Tile();
                t.TileType = (TileTypes)tileChars[i];
                switch (t.TileType)
                {
                    case TileTypes.Hero:
                    case TileTypes.GoldMine: //falls through
                        char c = tileChars[i + 1];
                        if (c != '-')
                        {
                            t.OwnerId = (byte)char.GetNumericValue(c);
                        }
                        break;
                }

                int position = i / 2;
                int line = position / size; //Integer math here will floor the number
                
                t.X = position - (line * size);
                t.Y = line;

                //Assign tile
                result[t.Y][t.X] = t;
            }
            return result;
        }

        public override string ToString()
        {
            switch(TileType)
            {
                case TileTypes.Open:
                    return "  ";
                case TileTypes.ImpassableWood:
                    return "##";
                case TileTypes.Tavern:
                    return "[]";
                case TileTypes.Hero:
                    return "@" + (OwnerId.HasValue ? OwnerId.ToString() : "?");
                case TileTypes.GoldMine:
                    return "$" + (OwnerId.HasValue ? OwnerId.ToString() : "-");
                default:
                    return "??";
            };
        }

        #endregion

        #region Node implementation

        public override int NodeMovementCost
        {
            get { return 1; }
        }

        public override bool NodeBlocked
        {
            get
            {
                switch (this.TileType)
                {
                    case TileTypes.Open:
                        return true;
                    //Can't get through any other type
                    default:
                        return false;
                }
            }
        }

        #endregion
    }
}
