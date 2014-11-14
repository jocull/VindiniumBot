using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VindiniumCore.GameTypes;
using VindiniumCore.PathFinding;

namespace VindiniumTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();

            //Test in closure
            {
                string gameJson = "{       \"id\":\"s2xh3aig\",       \"turn\":1100,       \"maxTurns\":1200,       \"heroes\":[          {             \"id\":1,             \"name\":\"vjousse\",             \"userId\":\"j07ws669\",             \"elo\":1200,             \"pos\":{                \"x\":5,                \"y\":6             },             \"life\":60,             \"gold\":0,             \"mineCount\":0,             \"spawnPos\":{                \"x\":5,                \"y\":6             },             \"crashed\":true          },          {             \"id\":2,             \"name\":\"vjousse\",             \"userId\":\"j07ws669\",             \"elo\":1200,             \"pos\":{                \"x\":12,                \"y\":6             },             \"life\":100,             \"gold\":0,             \"mineCount\":0,             \"spawnPos\":{                \"x\":12,                \"y\":6             },             \"crashed\":true          },          {             \"id\":3,             \"name\":\"vjousse\",             \"userId\":\"j07ws669\",             \"elo\":1200,             \"pos\":{                \"x\":12,                \"y\":11             },             \"life\":80,             \"gold\":0,             \"mineCount\":0,             \"spawnPos\":{                \"x\":12,                \"y\":11             },             \"crashed\":true          },          {             \"id\":4,             \"name\":\"vjousse\",             \"userId\":\"j07ws669\",             \"elo\":1200,             \"pos\":{                \"x\":4,                \"y\":8             },             \"lastDir\": \"South\",             \"life\":38,             \"gold\":1078,             \"mineCount\":6,             \"spawnPos\":{                \"x\":5,                \"y\":11             },             \"crashed\":false          }       ],       \"board\":{          \"size\":18,          \"tiles\":\"##############        ############################        ##############################    ##############################$4    $4############################  @4    ########################  @1##    ##    ####################  []        []  ##################        ####        ####################  $4####$4  ########################  $4####$4  ####################        ####        ##################  []        []  ####################  @2##    ##@3  ########################        ############################$-    $-##############################    ##############################        ############################        ##############\"       },       \"finished\":true    }";
                string heroJson = " { 	\"id\":1, 	\"name\":\"vjousse\", 	\"userId\":\"j07ws669\", 	\"elo\":1200, 	\"pos\":{ 	   \"x\":5, 	   \"y\":6 	}, 	\"lastDir\": \"South\", 	\"life\":60, 	\"gold\":0, 	\"mineCount\":0, 	\"spawnPos\":{ 	   \"x\":5, 	   \"y\":6 	}, 	\"crashed\":true  }";
                Game game = Game.FromJson(gameJson);
                Hero hero = Hero.FromJson(heroJson);
            }

            string gameStepJson = "{    \"game\":{       \"id\":\"s2xh3aig\",       \"turn\":1100,       \"maxTurns\":1200,       \"heroes\":[          {             \"id\":1,             \"name\":\"vjousse\",             \"userId\":\"j07ws669\",             \"elo\":1200,             \"pos\":{                \"x\":5,                \"y\":6             },             \"life\":60,             \"gold\":0,             \"mineCount\":0,             \"spawnPos\":{                \"x\":5,                \"y\":6             },             \"crashed\":true          },          {             \"id\":2,             \"name\":\"vjousse\",             \"userId\":\"j07ws669\",             \"elo\":1200,             \"pos\":{                \"x\":12,                \"y\":6             },             \"life\":100,             \"gold\":0,             \"mineCount\":0,             \"spawnPos\":{                \"x\":12,                \"y\":6             },             \"crashed\":true          },          {             \"id\":3,             \"name\":\"vjousse\",             \"userId\":\"j07ws669\",             \"elo\":1200,             \"pos\":{                \"x\":12,                \"y\":11             },             \"life\":80,             \"gold\":0,             \"mineCount\":0,             \"spawnPos\":{                \"x\":12,                \"y\":11             },             \"crashed\":true          },          {             \"id\":4,             \"name\":\"vjousse\",             \"userId\":\"j07ws669\",             \"elo\":1200,             \"pos\":{                \"x\":4,                \"y\":8             },             \"lastDir\": \"South\",             \"life\":38,             \"gold\":1078,             \"mineCount\":6,             \"spawnPos\":{                \"x\":5,                \"y\":11             },             \"crashed\":false          }       ],       \"board\":{          \"size\":18,          \"tiles\":\"##############        ############################        ##############################    ##############################$4    $4############################  @4    ########################  @1##    ##    ####################  []        []  ##################        ####        ####################  $4####$4  ########################  $4####$4  ####################        ####        ##################  []        []  ####################  @2##    ##@3  ########################        ############################$-    $-##############################    ##############################        ############################        ##############\"       },       \"finished\":true    },    \"hero\":{       \"id\":4,       \"name\":\"vjousse\",       \"userId\":\"j07ws669\",       \"elo\":1200,       \"pos\":{          \"x\":4,          \"y\":8       },       \"lastDir\": \"South\",       \"life\":38,       \"gold\":1078,       \"mineCount\":6,       \"spawnPos\":{          \"x\":5,          \"y\":11       },       \"crashed\":false    },    \"token\":\"lte0\",    \"viewUrl\":\"http://localhost:9000/s2xh3aig\",    \"playUrl\":\"http://localhost:9000/api/s2xh3aig/lte0/play\" }";
            GameState state = GameState.FromJson(gameStepJson);

            Console.WriteLine(state.Game.Board.ToString());
            Console.WriteLine(string.Format("{0:#,0} ms", sw.ElapsedMilliseconds));

            Hero myHero = state.Game.Heroes.First(x => x.ID == 1);
            Tile hero1Tile = state.Game.FindHeroes(x => x.OwnerId == 1).FirstOrDefault();
            Tile hero3Tile = state.Game.FindHeroes(x => x.OwnerId == 3).FirstOrDefault();

            IEnumerable<Tile> goldMines = state.Game.FindGoldMines(x => true);

            sw.Restart();
            PathFinder pathFinder = new PathFinder(state.Game.Board);

            var safeTravelFunc = new Func<Node, NodeStatus>(node => {
                Tile t = node as Tile;
                var neighbors = state.Game.Board.GetNeighboringNodes(t, 1, true).Select(x => x as Tile);
                foreach (var x in neighbors)
                {
                    //Any heros in the area?
                    if (x.TileType == Tile.TileTypes.Hero
                        && x.OwnerId != myHero.ID)
                    {
                        //Dangerous heros in the way?
                        Hero h = state.Game.LookupHero(x);
                        if (h != null)
                        {
                            //Avoid!
                            return new NodeStatus(30, true);
                        }
                    }
                }
                return new NodeStatus(1, false);
            });

            //Map all gold mines
            var pathsToGoldMines = state.Game.FindPathsToGoldMines(hero1Tile, x => true, safeTravelFunc).ToList();
            Console.WriteLine(string.Format("{0:#,0} ms", sw.ElapsedMilliseconds));

            foreach (var path in pathsToGoldMines)
            {
                sw.Restart();
                Console.WriteLine(state.Game.Board.ToString(path));
                Console.WriteLine(string.Format("{0:#,0} ms", sw.ElapsedMilliseconds));
            }

            sw.Restart();
            DirectionSet path2 = state.Game.FindPath(hero1Tile, hero3Tile);
            Console.WriteLine(string.Format("{0:#,0} ms", sw.ElapsedMilliseconds));
            Console.WriteLine(state.Game.Board.ToString(path2));

            Console.ReadLine();
        }
    }
}
