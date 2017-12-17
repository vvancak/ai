using System;
using System.Collections.Generic;
using System.Linq;

namespace TSP {
    class PermutationStandard
    {
        private static Random r = new Random();

        public TSPInput input;
        public int size;
        public int[] perm;

        public TSPSolution convertToTSPSol()
        {
            TSPSolution sol = new TSPSolution(input);
            for (int i = 0; i < size - 1; i++)
            {
                sol.setSuccessor(perm[i], perm[i + 1]);
            }
            sol.setSuccessor(perm[size - 1], perm[0]);
            return sol;
        }

        public void applyToTSPSolution(TSPSolution sol)
        {
            for (int i = 0; i < size - 1; i++)
            {
                sol.setSuccessor(perm[i], perm[i + 1]);
            }
            sol.setSuccessor(perm[size - 1], perm[0]);
        }

        public PermutationInversions convertToInversions()
        {
            PermutationInversions inv = new PermutationInversions(this.input)
            {
                size = this.size,
                perm = new int[this.size]
            };
            for (int i = 0; i < this.size; i++)
            {
                int count = 0, m = 0;
                while (this.perm[m] != i)
                {
                    if (this.perm[m] > i)
                        count++;
                    m++;
                }
                inv.perm[i] = count;
            }
            return inv;
        }

        public PermutationStandard(TSPInput input)
        {
            this.size = input.nodesCount;
            this.perm = new int[size];
            this.input = input;
        }

        public PermutationStandard(TSPSolution sol) : this(sol.inp)
        {
            perm[0] = 0;
            perm[1] = sol.getSuccessor(perm[0]);
            for (int i = 1; i < size - 1; i++)
            {
                perm[i + 1] = sol.getSuccessor(perm[i], perm[i - 1]);
            }
        }

        public void swap(int i, int j)
        {
            int k = perm[i];
            perm[i] = perm[j];
            perm[j] = k;
        }

        public void randomize()
        {
            List<int> notUsed = new List<int>();
            for (int i = 0; i < size; i++)
            {
                notUsed.Add(i);
            }
            for (int i = 0; i < size; i++)
            {
                int selectedIndex = PermutationStandard.r.Next(notUsed.Count);
                perm[i] = notUsed[selectedIndex];
                notUsed.RemoveAt(selectedIndex);
            }
        }

        public void randomize2()
        {
            int distanceused = 0, allUsed = 0;
            List<int> notUsed = new List<int>();
            for (int i = 1; i < size; i++)
            {
                notUsed.Add(i);
            }
            perm[0] = 0;

            double distanceSum = 0;
            int count = 0;
            for (int i = 0; i < size; i++)
            {
                for (int j = i + 1; j < size; j++)
                {
                    distanceSum += input.getDistance(i, j);
                    count++;
                }
            }
            double distanceLimit = (distanceSum / count) / 2;

            for (int i = 1; i < size; i++)
            {
                var nearByCities = notUsed.Where(a => input.getDistance(perm[i - 1], a) < distanceLimit);
                if (nearByCities.Count() == 0)
                {
                    allUsed++;
                    int selectedIndex = PermutationStandard.r.Next(notUsed.Count);
                    perm[i] = notUsed[selectedIndex];
                    notUsed.RemoveAt(selectedIndex);
                }
                else
                {
                    distanceused++;
                    int selectedIndex = PermutationStandard.r.Next(nearByCities.Count());
                    perm[i] = nearByCities.ToList()[selectedIndex];
                    List<int> ff = nearByCities.ToList();
                    notUsed.Remove(perm[i]);
                }
            }
        }

        public double eval()
        {
            double totalDistance = 0;
            for (int i = 0; i < size - 1; i++)
            {
                totalDistance += input.getDistance(perm[i], perm[i + 1]);
            }
            totalDistance += input.getDistance(perm[size - 1], perm[0]);
            return totalDistance;
        }
    }
}