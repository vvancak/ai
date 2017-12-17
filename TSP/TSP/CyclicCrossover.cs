using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Windows.Forms;

namespace TSP
{
    class CyclicCrossover
    {
        class Cycle
        {
            struct Edge
            {
                public int From;
                public int To;
            }

            private List<Edge> cycle;

            public Cycle()
            {
                this.cycle = new List<Edge>();
            }

            public void Add(int from, int to)
            {
                cycle.Add(new Edge {From = from, To = to});
            }

            public IEnumerable<int> GetStartingPoints()
            {
                for (int i = 0; i < cycle.Count; ++i) yield return cycle[i].From;
            }

            public bool IsComplete()
            {
                if (cycle.Count == 0) return false;
                var lastEdge = cycle[cycle.Count - 1];
                var firstEdge = cycle[0];
                return firstEdge.From == lastEdge.To;
            }
        }

        public static TSPSolution GetNew(TSPSolution parent1, TSPSolution parent2)
        {
            PermutationStandard ps1 = new PermutationStandard(parent1);
            PermutationStandard ps2 = new PermutationStandard(parent2);
            PermutationStandard tmp = new PermutationStandard(parent2);                   

            // Keep track of elements, which have not been moved yet
            List<bool> moved = new List<bool>(tmp.size);
            for (int i = 0; i < tmp.size; ++i) moved.Add(false);

            // starting point of the cycle - first unmoved element
            moved[0] = true;

            while (moved.Contains(false))
            {
                Cycle cycle = new Cycle();

                int start = moved.IndexOf(false);

                // find the cycle
                while (!cycle.IsComplete())
                {
                    int through = ps2.perm[start];
                    int to = 0;
                    for (int i = 0; i < ps1.perm.Length; ++i)
                        if (ps1.perm[i] == through) to = i;

                    cycle.Add(start, to);
                    start = to;
                }

                // elements from ps1 where the cycle goes
                foreach (var startingPoint in cycle.GetStartingPoints())
                {
                    tmp.perm[startingPoint] = ps1.perm[startingPoint];
                    moved[startingPoint] = true;
                }

                // swap ps1 and ps2
                var swap = ps1;
                ps1 = ps2;
                ps2 = swap;
            }

           return tmp.convertToTSPSol();       
        }
    }
}