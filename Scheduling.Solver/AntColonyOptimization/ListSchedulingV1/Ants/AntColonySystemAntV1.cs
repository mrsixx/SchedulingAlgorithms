using Scheduling.Core.Extensions;
using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Algorithms;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Ants
{
    public class AntColonySystemAntV1(int id, int generation, AntColonySystemAlgorithmV1 context) : AntV1<AntColonySystemAntV1, AntColonySystemAlgorithmV1>(id, generation, context)
    {

        public override void WalkAround()
        {
            ListSchedulingV1Heuristic<AntColonySystemAlgorithmV1, AntColonySystemAntV1>.Construct(this);
        }

        /// <summary>
        /// Create feasible moves set and use pseudo probability rule to choose one
        /// </summary>
        /// <returns></returns>
        public override IFeasibleMove<Orientation> ProbabilityRule(IEnumerable<IFeasibleMove<Orientation>> feasibleMoves)
        {
            return PseudoProbabilityRule(feasibleMoves);
        }

        private IFeasibleMove<Orientation> PseudoProbabilityRule(IEnumerable<IFeasibleMove<Orientation>> feasibleMoves)
        {
            var sum = 0.0;
            var rouletteWheel = new List<(IFeasibleMove<Orientation> Move, double Probability)>();
            IFeasibleMove<Orientation> greedyMove = null;
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

        public override void LocalPheromoneUpdate(Orientation selectedMove)
        {
            if (!context.PheromoneStructure.TryGetValue(selectedMove, out double currentPheromoneValue) ||
                !context.PheromoneStructure.TryUpdate(selectedMove, (1 - context.Phi) * currentPheromoneValue + context.Phi * context.Parameters.Tau0, currentPheromoneValue))
                Console.WriteLine("Unable to decay pheromone after construction step...");
        }
    }
}
