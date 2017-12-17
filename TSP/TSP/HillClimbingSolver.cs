using System;
using System.Text;

namespace TSP
{
    class HillClimbingSolver : TSPSolver
    {
        private static Random r = new Random();
        private TSPVisualizer visualizer;
        private TSPSolution currentBest;
        private PermutationStandard current;
        private bool stop;
        private int stepsWithoutImprovement = 0;
        Random rnd = new Random(31081996);

        public TSPSolution solve(TSPInput input)
        {
            Console.WriteLine("Hill climbing started");
            current = initialize(input);
            currentBest = current.convertToTSPSol();
            visualizer.draw(currentBest);
            stop = false;
            int steps = 0;
            while (!stop)
            {
                steps++;
                goOneStep();
                if (steps % 10 == 0)
                {
                    currentBest = current.convertToTSPSol();
                    visualizer.draw(currentBest);
                    Console.WriteLine("Steps: " + steps + " Best distance: " + currentBest.totalDistance);
                }
            }
            Console.WriteLine("Search ended");
            currentBest = current.convertToTSPSol();
            visualizer.draw(currentBest);
            Console.WriteLine("Steps: " + steps + " Best distance: " + currentBest.totalDistance);
            return currentBest;
        }

        /// <summary>
        /// Tato metoda by mela vylepsit reseni ulozene v promenne "current". Kvalitu reseni je mozne ziskat volanim "current.eval()". Je mozne napr. prohodit nektere prvky v permutaci pomoci metody "current.swap(int i, int j)".
        /// Pokud uz soucasne reseni nelze vylepsit, metoda by mela nastavit promenou "stop" na "true", coz zpusobi, ze prohledavani skonci.
        /// Pro vylepseni (nebo obecne zmenu) reseni pouzijte nekterou variantu Hill-Climbingu.
        /// </summary>
        private void goOneStepII()
        {
            var start = current.eval();
            var best = start;

            var bestX = 0;
            var bestY = 0;

            for (int i = 0; i < current.perm.Length; ++i)
            for (int j = 0; j < current.perm.Length; ++j)
            {
                current.swap(i, j);
                var temp = current.eval();
                current.swap(j, i);

                if (temp < best)
                {
                    best = temp;
                    bestX = i;
                    bestY = j;
                }
            }

            if (best >= start) stop = true;
            else current.swap(bestX, bestY);
        }

        private void goOneStep()
        {
            int prob = 30;

            var start = current.eval();
            var best = start;

            var bestX = 0;
            var bestY = 0;

            for (int i = 0; i < current.perm.Length; ++i)
            for (int j = 0; j < current.perm.Length; ++j)
            {
                current.swap(i, j);
                var temp = current.eval();
                current.swap(j, i);

                if (temp < best)
                {
                    temp = tryDoPermutation(prob);
                    best = temp;
                    bestX = i;
                    bestY = j;
                }
            }

            if (best >= start) stop = true;
            else current.swap(bestX, bestY);
        }

        private double tryDoPermutation(int probability)
        {
            var start = current.eval();
            var best = start;

            for (int i = 0; i < current.perm.Length; ++i)
            for (int j = 0; j < current.perm.Length; ++j)
            {
                current.swap(i, j);
                var temp = current.eval();


                if (temp < best)
                {
                    if (rnd.Next(100) < probability) temp = tryDoPermutation(probability / 2);
                    best = temp;
                }
                current.swap(j, i);
            }

            if (best >= start) return start;
            return best;
        }

        private PermutationStandard initialize(TSPInput input)
        {
            var result = new PermutationStandard(input);
            result.randomize();
            return result;
        }

        private PermutationStandard initialize2(TSPInput input)
        {
            var solver = new GreedySolver();
            var result = solver.solve(input);
            return new PermutationStandard(result);
        }

        public HillClimbingSolver(TSPVisualizer visualizer)
        {
            this.visualizer = visualizer;
        }
    }

    class PermutationInversions
    {
        public int size;
        public int[] perm;
        public TSPInput input;

        public PermutationStandard convertToStandard()
        {
            PermutationStandard std = new PermutationStandard(this.input);
            int[] pos = new int[this.size];
            for (int i = size - 1; i >= 0; i--)
            {
                for (int m = i; m < size; m++)
                {
                    if (pos[m] >= this.perm[i] + 1)
                        pos[m]++;
                    pos[i] = perm[i] + 1;
                }
            }
            for (int i = 0; i < size; i++)
            {
                std.perm[pos[i] - 1] = i;
            }
            return std;
        }

        public TSPSolution convertToTSPSol()
        {
            return this.convertToStandard().convertToTSPSol();
        }

        public PermutationInversions(TSPInput input)
        {
            this.size = input.nodesCount;
            this.perm = new int[size];
        }

        public double eval()
        {
            return convertToStandard().eval();
            //return convertToTSPSol().totalDistance;
        }
    }
}