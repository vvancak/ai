using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSP
{
    class GeneticAlgorightm : TSPSolver
    {
        private TSPVisualizer visualizer;

        private Random rnd = new Random(31081996);
        private RouletteWheelSelector bestSelector;
        private RouletteWheelSelector worstSelector;

        private List<TSPSolution> population;

        private const int PopulationSize = 300;
        private const int Generations = 300;
        private const int MutationChance = 25;

        private int generationTransferCount;
        private int dropoutZone;

        public GeneticAlgorightm(TSPVisualizer visualizer)
        {
            population = new List<TSPSolution>(PopulationSize);
            bestSelector = new RouletteWheelSelector(rnd, Fitness);
            worstSelector = new RouletteWheelSelector(rnd, s => 1 / Fitness(s));
            this.visualizer = visualizer;
        }

        public TSPSolution solve(TSPInput input)
        {
            TSPSolution best = null;
            for (int r = 0; r < 5; ++r)
            {
                Console.WriteLine($@"== RUN:{r} ==");
                population.Clear();
                for (int i = 0; i < PopulationSize; ++i) population.Add(getRandomSolution(input));
                run();

                var tmp = GetBest();
                Console.WriteLine($@"Distance: {tmp.totalDistance}");

                if (best == null || tmp.totalDistance < best.totalDistance) best = tmp;
            }
            visualizer.draw(best);
            return best;
        }

        private void run()
        {
            for (int i = 0; i < Generations; ++i)
            {
                generationTransferCount = (int) (population.Count * (1 / 3.0));
                dropoutZone = population.Count - generationTransferCount;

                List<TSPSolution> nextPopulation = new List<TSPSolution>(population.Count);
                bestSelector.Transfer(generationTransferCount, population, nextPopulation);
                population = nextPopulation;

                mating();

                mutatePopulation();

                Console.WriteLine($@"Generation #{i}; Best: {GetBest().computeDistance()}");
                if (i % 10 == 0) visualizer.draw(GetBest());
            }
        }

        // Best element from the population (i.e. shortest path)
        private TSPSolution GetBest()
        {
            return population.OrderByDescending(Fitness).First();
        }

        private double Fitness(TSPSolution sol)
        {
            return 1 / sol.computeDistance();
        }

        private void mating()
        {
            for (int i = 0; i < dropoutZone / 2; ++i)
            {
                TSPSolution parent1 = bestSelector.SelectOne(population);
                TSPSolution parent2 = bestSelector.SelectOne(population);

                var new1 = CyclicCrossover.GetNew(parent1, parent2);
                var new2 = CyclicCrossover.GetNew(parent2, parent1);

                population.Add(new1);
                population.Add(new2);
            }
        }

        private void mutatePopulation()
        {
            while (rnd.Next(100) < MutationChance / 2) mutateSmart(worstSelector.SelectOne(population));
            while (rnd.Next(100) < MutationChance / 2) mutateDumb(worstSelector.SelectOne(population));
        }

        // Initialization of a TSPSolution
        private TSPSolution getRandomSolution(TSPInput input)
        {
            var perm = new PermutationStandard(input);
            perm.randomize();
            var p = perm.convertToTSPSol();
            if (!p.validate()) throw new InvalidOperationException();
            return p;
        }

        // "Smart mutation" - try to swap and improve the distance
        private void mutateSmart(TSPSolution initial)
        {
            PermutationStandard current = new PermutationStandard(initial);

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

            if (best < start)
            {
                current.swap(bestX, bestY);
                current.applyToTSPSolution(initial);
            }
        }

        // "Dumb mutation" - just swap some elements
        private void mutateDumb(TSPSolution initial)
        {
            PermutationStandard current = new PermutationStandard(initial);
            current.swap(rnd.Next(current.size), rnd.Next(current.size));
            current.applyToTSPSolution(initial);
        }
    }
}