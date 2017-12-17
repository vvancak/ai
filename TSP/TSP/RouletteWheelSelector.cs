using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSP
{
    class RouletteWheelSelector
    {
        private Random rnd;
        private Func<TSPSolution, double> fitness;

        public RouletteWheelSelector(Random rnd, Func<TSPSolution, double> fitness)
        {
            this.rnd = rnd;
            this.fitness = fitness;
        }

        public TSPSolution SelectOne(List<TSPSolution> from)
        {
            var slot = GetBestIndex(from);
            return from[slot];
        }

        public void Transfer(int count, List<TSPSolution> from, List<TSPSolution> to)
        {
            for (int i = 0; i < count; ++i)
            {
                var slot = GetBestIndex(from);
                to.Add(from[slot]);
                from.RemoveAt(slot);
            }
        }

        private int GetBestIndex(List<TSPSolution> from)
        {
            // generate the wheel
            double totalFitness = 0;
            foreach (var s in from) totalFitness += fitness(s);

            // shoot at the wheel
            int slot = 0;
            double shotPercentage = rnd.NextDouble();

            // figure out where the shot landed
            while (shotPercentage > 0 && slot < from.Count)
            {
                var slotFitness = fitness(from[slot++]);
                var slotPercentage = slotFitness / totalFitness;
                shotPercentage -= slotPercentage;
            }

            // return the element's index
            return (slot - 1);
        }
    }
}