using System;
using TwoPhaseSolver;

namespace SolverTest
{
    
    class Program
    {
                static void Main(string[] args)
        {
            int i;
            /*
            
            Cube c = Move.randmove(50).apply(new Cube());
            for (i = 0; i < 8; i++)
            {
                Console.WriteLine("corner " + i + " --> " + c.corners[i]);
            }
            for (i = 0; i < 12; i++)
            {
                Console.WriteLine("edge " + i + " --> " + c.edges[i]);
            }
            */
            
            string input = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Oggetti_output.txt");                            //directory con blocchi
            char[] delimiter = { '/' };
            int[] blocco, orientamento;
            blocco = new int[19];
            orientamento = new int[19];

            string[] inp_aux_1 = input.Split('/');
            blocco = Array.ConvertAll<string, int>(inp_aux_1, int.Parse);

            input = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Orientamenti_output.txt");                                       //directory con orientamenti
            string[] inp_aux_2 = input.Split('/');
            orientamento = Array.ConvertAll<string, int>(inp_aux_2, int.Parse);
            /*
            for (i = 0; i < 20; i++)
            {
                Console.WriteLine("blocks " + i + " --> " + blocco[i]);
                Console.WriteLine("orien " + i + " --> " + orientamento[i]);
            }
             */          
            Cubie[] edges, corns;
            corns = new Cubie[8];
            edges = new Cubie[12];

            corns[0] = new Cubie((byte)blocco[0],  (byte)orientamento[0]);
            corns[1] = new Cubie((byte)blocco[1],  (byte)orientamento[1]);
            corns[2] = new Cubie((byte)blocco[2],  (byte)orientamento[2]);
            corns[3] = new Cubie((byte)blocco[3],  (byte)orientamento[3]);
            corns[4] = new Cubie((byte)blocco[4],  (byte)orientamento[4]);
            corns[5] = new Cubie((byte)blocco[5],  (byte)orientamento[5]);
            corns[6] = new Cubie((byte)blocco[6],  (byte)orientamento[6]);
            corns[7] = new Cubie((byte)blocco[7],  (byte)orientamento[7]);
            edges[0] = new Cubie((byte)blocco[8],  (byte)orientamento[8]);
            edges[1] = new Cubie((byte)blocco[9],  (byte)orientamento[9]);
            edges[2] = new Cubie((byte)blocco[10], (byte)orientamento[10]);
            edges[3] = new Cubie((byte)blocco[11], (byte)orientamento[11]);
            edges[4] = new Cubie((byte)blocco[12], (byte)orientamento[12]);
            edges[5] = new Cubie((byte)blocco[13], (byte)orientamento[13]);
            edges[6] = new Cubie((byte)blocco[14], (byte)orientamento[14]);
            edges[7] = new Cubie((byte)blocco[15], (byte)orientamento[15]);
            edges[8] = new Cubie((byte)blocco[16], (byte)orientamento[16]);
            edges[9] = new Cubie((byte)blocco[17], (byte)orientamento[17]);
            edges[10] = new Cubie((byte)blocco[18], (byte)orientamento[18]);
            edges[11] = new Cubie((byte)blocco[19], (byte)orientamento[19]);

            Cube g = new Cube(corns,edges );  //----> usare questo cubo al posto di c
            
            /*
            for (i = 0; i < 8; i++)
            {
                Console.WriteLine("corner " + i + " --> " + g.corners[i]);
            }
            for (i = 0; i < 12; i++)
            {
                Console.WriteLine("edge " + i + " --> " + g.edges[i]);
            }
            */
            Search.fullSolve(g, 30, 6000, true);

            Environment.Exit(0);
            
            //Console.Write("Press any key to continue...");
            //Console.Read();
        }
    }
}
