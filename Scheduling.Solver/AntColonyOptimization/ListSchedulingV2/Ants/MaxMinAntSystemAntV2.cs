using System.Runtime.CompilerServices;
using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Algorithms;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Ants
{
    public class MaxMinAntSystemAntV2(int id, int generation, MaxMinAntSystemAlgorithmV2 context)
        : AntV2<MaxMinAntSystemAntV2, MaxMinAntSystemAlgorithmV2>(id, generation, context)
    {
        public override double Makespan => CompletionTimes.Any() ? CompletionTimes.MaxBy(c => c.Value).Value : 0;

        protected Dictionary<Machine, LinkedList<Operation>> LoadingSequence { get; } = [];
        public override void WalkAround()
        {
            // creating data structures
            var unscheduledOperations = new HashSet<Operation>();
            Instance.Jobs.ToList().ForEach(job => unscheduledOperations.Add(job.Operations.First.Value));
            Instance.Machines.ToList().ForEach(m => LoadingSequence.Add(m, new LinkedList<Operation>()));

            while (unscheduledOperations.Any())
            {
                var feasibleMoves = GetFeasibleMoves(unscheduledOperations);
                var nextMove = ProbabilityRule(feasibleMoves) as FeasibleMoveV2;

                var operationLinkedListNode = nextMove.Operation.Job.Operations.Find(nextMove.Operation);

                EvaluateCompletionTime(nextMove, operationLinkedListNode);
                LocalPheromoneUpdate(nextMove.Allocation);

                unscheduledOperations.Remove(nextMove.Operation);
                if (operationLinkedListNode.Next is not null)
                    unscheduledOperations.Add(operationLinkedListNode.Next.Value);
            }
            //Log($"Solution found after {solution.Watch.Elapsed}.");
            //Log("\nFinishing execution...");

            // creating mu function
            foreach (var (m, operations) in LoadingSequence)
            foreach (var o in operations)
                MachineAssignment.Add(o.Id, m);
        }

        public void EvaluateCompletionTime(FeasibleMoveV2 selectedMove, LinkedListNode<Operation> operationLinkedListNode)
        {
            // evaluate start e completion times
            var machinePredecessor = LoadingSequence[selectedMove.Machine].Last;


            var jobPredecessor = operationLinkedListNode.Previous;
            var jobReleaseDate = Convert.ToDouble(selectedMove.Operation.Job.ReleaseDate);

            var startTime = Math.Max(
                machinePredecessor != null ? CompletionTimes[machinePredecessor.Value.Id] : 0,
                jobPredecessor != null ? CompletionTimes[jobPredecessor.Value.Id] : jobReleaseDate
            );

            Path.Add(selectedMove.Allocation);
            StartTimes.TryAdd(selectedMove.Operation.Id, startTime);
            CompletionTimes.TryAdd(selectedMove.Operation.Id, startTime + selectedMove.Operation.GetProcessingTime(selectedMove.Machine));

            // updating data structures
            LoadingSequence[selectedMove.Machine].AddLast(selectedMove.Operation);
        }

        public virtual IFeasibleMove<Allocation> ProbabilityRule(IEnumerable<IFeasibleMove<Allocation>> feasibleMoves)
        {
            var sum = 0.0;
            var rouletteWheel = new List<(IFeasibleMove<Allocation> Move, double Probability)>();

            // create roulette wheel and evaluate greedy move for pseudorandom proportional rule at same time (in O(n))
            foreach (var move in feasibleMoves)
            {
                var tauXy = move.GetPheromoneAmount(Context.PheromoneTrail); // pheromone amount
                var etaXy = move.Weight.Inverse(); // heuristic information
                var tauXyAlpha = Math.Pow(tauXy, Context.Parameters.Alpha); // pheromone amount raised to power alpha
                var etaXyBeta = Math.Pow(etaXy, Context.Parameters.Beta); // heuristic information raised to power beta

                double probFactor = tauXyAlpha * etaXyBeta;
                rouletteWheel.Add((move, probFactor));
                sum += probFactor;
            }

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
    }
}
