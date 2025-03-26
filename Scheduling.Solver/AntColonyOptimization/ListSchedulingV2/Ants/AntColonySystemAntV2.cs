using Scheduling.Core.Extensions;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Algorithms;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Ants
{
    public class AntColonySystemAntV2(int id, int generation, AntColonySystemAlgorithmV2 context) : AntV2<AntColonySystemAntV2, AntColonySystemAlgorithmV2>(id, generation, context)
    {

        public override void WalkAround()
        {
            ListSchedulingV2Heuristic<AntColonySystemAlgorithmV2, AntColonySystemAntV2>.Construct(this);
        }

        /// <summary>
        /// Create feasible moves set and use pseudo probability rule to choose one
        /// </summary>
        /// <returns></returns>
        public override IFeasibleMove<Allocation> ProbabilityRule(IEnumerable<IFeasibleMove<Allocation>> feasibleMoves)
        {
            return PseudoProbabilityRule(feasibleMoves);
        }

        private IFeasibleMove<Allocation> PseudoProbabilityRule(IEnumerable<IFeasibleMove<Allocation>> feasibleMoves)
        {
            var sum = 0.0;
            var rouletteWheel = new List<(IFeasibleMove<Allocation> Move, double Probability)>();
            IFeasibleMove<Allocation> greedyMove = null;
            var greedyFactor = double.MinValue;
            // create roulette wheel and evaluate greedy move for pseudorandom proportional rule at same time (in O(n))
            foreach (var move in feasibleMoves)
            {
                var tauXy = move.GetPheromoneAmount(context.PheromoneTrail); // pheromone amount
                var etaXy = move.Weight.Inverse(); // heuristic information
                var tauXyAlpha = Math.Pow(tauXy, context.Parameters.Alpha); // pheromone amount raised to power alpha
                var etaXyBeta = Math.Pow(etaXy, context.Parameters.Beta); // heuristic information raised to power beta

                double probFactor = tauXyAlpha * etaXyBeta, pseudoProbFactor = tauXy * etaXyBeta;
                rouletteWheel.Add((move, probFactor));
                sum += probFactor;

                if (greedyFactor >= pseudoProbFactor) continue;
                greedyFactor = pseudoProbFactor;
                greedyMove = move;
            }

            // pseudo random proportional rule
            if (Random.Shared.NextDouble() <= context.Q0)
                return greedyMove;

            // roulette wheel
            var cumulative = 0.0;
            var randomValue = Random.Shared.NextDouble() * sum;

            foreach (var (move, probability) in rouletteWheel)
            {
                cumulative += probability;
                if (randomValue <= cumulative)
                    return move;
            }

            throw new InvalidOperationException("FATAL ERROR: No move was selected.");
        }

        public override void LocalPheromoneUpdate(Allocation selectedMove)
        {
            if (!context.PheromoneTrail.TryGetValue(selectedMove, out double currentPheromoneValue) ||
                !context.PheromoneTrail.TryUpdate(selectedMove, (1 - context.Phi) * currentPheromoneValue + context.Phi * context.Parameters.Tau0, currentPheromoneValue))
                Console.WriteLine("Unable to decay pheromone after construction step...");
        }
    }
}
