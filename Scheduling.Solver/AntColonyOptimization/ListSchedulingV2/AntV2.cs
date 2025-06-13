using Scheduling.Core.FJSP;
using Scheduling.Core.Extensions;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV2
{
    public abstract class AntV2<TSelf, TContext> : BaseAnt<TSelf>
        where TSelf : AntV2<TSelf, TContext>
        where TContext : AntColonyV2AlgorithmSolver<TContext, TSelf>
    {
        protected AntV2(int id, int generation, TContext context)
        {
            Id = id;
            Generation = generation;
            Context = context;
        }

        public TContext Context { get; }

        public Instance Instance => Context.Instance;

        public HashSet<Allocation> Path { get; } = [];

        public double Makespan => Solution.Makespan;

        public override void Log()
        {
            // print loading sequence
            //foreach (var machine in LoadingSequence.Keys)
            //{
            //    Console.Write($"{machine.Id}: ");
            //    var operation = LoadingSequence[machine].First;
            //    while(operation != null){
            //        Console.Write($" {operation.Value.Id}[{StartTimes[operation.Value.Id]}-{CompletionTimes[operation.Value.Id]}] ");
            //        operation = operation.Next;
            //    }
                
            //    Console.WriteLine("");
                Console.WriteLine($"Makespan: {Makespan}");
            //}
        }

        public virtual void LocalPheromoneUpdate(Allocation selectedMove) { }

        public virtual IFeasibleMove<Allocation> ProbabilityRule(IEnumerable<IFeasibleMove<Allocation>> feasibleMoves)
        {
            var sum = 0.0;
            var rouletteWheel = new List<(IFeasibleMove<Allocation> Move, double Probability)>();

            // create roulette wheel and evaluate greedy move for pseudorandom proportional rule at same time (in O(n))
            foreach (var move in feasibleMoves)
            {
                var tauXy = move.GetPheromoneAmount(Context.PheromoneStructure); // pheromone amount
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

        public IEnumerable<IFeasibleMove<Allocation>> GetFeasibleMoves(HashSet<Operation> unscheduledNodes)
        {
            return unscheduledNodes.SelectMany(candidateNode =>
                candidateNode.EligibleMachines.Select(m => new FeasibleMoveV2(candidateNode, m)));
        }

        public void EvaluateCompletionTime(AntSolution antSolution, Allocation selectedMove)
        {
            // evaluate start e completion times
            var machinePredecessor = antSolution.LoadingSequence[selectedMove.Machine.Index].LastOrDefault();


            var jobPredecessor = !selectedMove.Operation.FirstOperation 
                ? selectedMove.Operation.Job.Operations[selectedMove.Operation.Index - 1] 
                : null;

            var jobReleaseDate = Convert.ToDouble(selectedMove.Operation.Job.ReleaseDate);

            var startTime = Math.Max(
                machinePredecessor != null ? antSolution.CompletionTimes[machinePredecessor.Id] : 0,
                jobPredecessor != null ? antSolution.CompletionTimes[jobPredecessor.Id] : jobReleaseDate
            );

            Path.Add(selectedMove);
            antSolution.StartTimes.TryAdd(selectedMove.Operation.Id, startTime);
            antSolution.CompletionTimes.TryAdd(selectedMove.Operation.Id, startTime + selectedMove.Operation.GetProcessingTime(selectedMove.Machine));

            // updating data structures
            antSolution.LoadingSequence[selectedMove.Machine.Index].Add(selectedMove.Operation);
        }
    }
}
