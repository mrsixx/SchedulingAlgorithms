
using Scheduling.Core.Extensions;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Algorithms;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Ants
{
    public class AntColonySystemAntV3(int id, int generation, AntColonySystemAlgorithmV3 context) : AntV3<AntColonySystemAntV3, AntColonySystemAlgorithmV3>(id, generation, context)
    {
        public override void WalkAround()
        {
            ListSchedulingV3Heuristic<AntColonySystemAlgorithmV3, AntColonySystemAntV3>.Construct(this);
        }

        /// <summary>
        /// Create feasible moves set and use pseudo probability rule to choose one
        /// </summary>
        /// <returns></returns>
        public override FeasibleMoveV3 ProbabilityRule(IEnumerable<FeasibleMoveV3> feasibleMoves)
        {
            return PseudoProbabilityRule(feasibleMoves);
        }

        private FeasibleMoveV3 PseudoProbabilityRule(IEnumerable<FeasibleMoveV3> feasibleMoves)
        {
            var sum = 0.0;
            var rouletteWheel = new List<(FeasibleMoveV3 Move, double Probability)>();
            FeasibleMoveV3 greedyMove = null;
            var greedyFactor = double.MinValue;
            // create roulette wheel and evaluate greedy move for pseudorandom proportional rule at same time (in O(n))
            foreach (var move in feasibleMoves)
            {
                var tauXy = move.GetPheromoneAmount(context.PheromoneStructure); // pheromone amount
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

        public override void LocalPheromoneUpdate(FeasibleMoveV3 selectedMove)
        {
            if (!context.PheromoneStructure.TryGetValue(selectedMove.Allocation, out double currentPheromoneValue) ||
                !context.PheromoneStructure.TryUpdate(selectedMove.Allocation, (1 - context.Phi) * currentPheromoneValue + context.Phi * context.Parameters.Tau0, currentPheromoneValue))
                Console.WriteLine("Unable to decay pheromone after construction step...");
        }
    }
}
