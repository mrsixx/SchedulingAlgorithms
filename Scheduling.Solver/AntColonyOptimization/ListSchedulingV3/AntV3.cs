using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3
{
    public abstract class AntV3<TSelf, TContext> : BaseAnt<TSelf>
        where TSelf : AntV3<TSelf, TContext>
        where TContext : AntColonyV3AlgorithmSolver<TContext, TSelf>
    {
        protected AntV3(int id, int generation, TContext context)
        {
            Id = id;
            Generation = generation;
            Context = context;
        }

        public TContext Context { get; }

        public virtual Dictionary<Machine, List<Operation>> LoadingSequence { get; } = [];

        public double Makespan => Solution.Makespan;

        public PrecedenceDigraph PrecedenceDigraph => Context.PrecedenceDigraph;

        public HashSet<Allocation> Allocations { get; } = [];

        public void InitializeDataStructures()
        {
            // Initialize starting and completion times for each operation
            PrecedenceDigraph.VertexSet.ForEach(vertex =>
            {
                Solution.CompletionTimes.Add(vertex.Id, 0);
                Solution.StartTimes.Add(vertex.Id, 0);
            });

            // Initialize loading sequences for each machine
            PrecedenceDigraph.Instance.Machines.ForEach(machine =>
                LoadingSequence.Add(machine, [])
            );
        }

        public void EvaluateCompletionTime(FeasibleMoveV3 selectedMove)
        {
            // evaluate start e completion times
            var machinePredecessor = LoadingSequence[selectedMove.Machine].LastOrDefault();
            var criticalPredecessor =
                PrecedenceDigraph
                    .NeighbourhoodIn(selectedMove.Vertex)
                    .MaxBy(predecessor => Solution.CompletionTimes[predecessor.Operation.Id]);

            var jobReleaseDate = Convert.ToDouble(selectedMove.Operation.Job.ReleaseDate);

            var startTime = Math.Max(
                machinePredecessor != null ? Solution.CompletionTimes[machinePredecessor.Id] : 0,
                criticalPredecessor != null ? Solution.CompletionTimes[criticalPredecessor.Operation.Id] : jobReleaseDate
            );

            Allocations.Add(selectedMove.Allocation);
            Solution.StartTimes[selectedMove.Operation.Id] = startTime;
            Solution.CompletionTimes[selectedMove.Operation.Id] = startTime + selectedMove.Operation.GetProcessingTime(selectedMove.Machine);

            // updating data structures
            LoadingSequence[selectedMove.Machine].Add(selectedMove.Operation);
        }

        public virtual FeasibleMoveV3 ProbabilityRule(IEnumerable<FeasibleMoveV3> feasibleMoves)
        {
            var sum = 0.0;
            var rouletteWheel = new List<(FeasibleMoveV3 Move, double Probability)>();

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

        public IEnumerable<FeasibleMoveV3> GetFeasibleMoves(HashSet<OperationVertex> unscheduledNodes, HashSet<OperationVertex> scheduledNodes)
        {
            return unscheduledNodes.SelectMany(candidateOperation =>
                candidateOperation.Operation.EligibleMachines.Select(m => new FeasibleMoveV3(candidateOperation, m)));
        }

        public virtual void LocalPheromoneUpdate(FeasibleMoveV3 selectedMove) { }

        public override void Log()
        {
            Console.WriteLine("");
            Console.WriteLine($"Makespan: {Makespan}");
        }

    }
}
