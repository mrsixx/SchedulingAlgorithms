using QuikGraph;
using QuikGraph.Algorithms;
using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Solver.Algorithms;
using Scheduling.Solver.Utils;
using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Solver.AntColonyOptimization
{
    public class Ant
    {
        public Ant(int id, int generation, AntColonyOptimizationAlgorithmSolver context)
        {
            Id = id;
            Context = context;
            Generation = generation;
            Context.DisjunctiveGraph.Machines.ForEach(m => {
                if (!LoadingSequence.ContainsKey(m))
                    LoadingSequence.Add(m, new Stack<Node>([Context.DisjunctiveGraph.Source]));
            });
            Task = new Task(() => WalkAround());
            Task.Start();
        }

        public int Id { get; }

        public int Generation { get; }

        public ConjunctiveGraphModel ConjunctiveGraph { get; } = new ConjunctiveGraphModel();

        public Dictionary<Machine, Stack<Node>> LoadingSequence { get; } = [];

        public Dictionary<Operation, double> CompletionTimes { get; } = [];

        public double Makespan => CompletionTimes.MaxBy(o => o.Value).Value;

        public Node StartNode => Context.DisjunctiveGraph.Source;

        public Node FinalNode => Context.DisjunctiveGraph.Sink;

        public AntColonyOptimizationAlgorithmSolver Context { get; }

        public Task Task { get; }

        private void WalkAround()
        {
            var remainingNodes = Context.DisjunctiveGraph.OperationVertices.ToList();
            Context.DisjunctiveGraph.Vertices.ToList().ForEach(n => CompletionTimes.Add(n.Operation, 0));
            var remainingDisjunctions = Context.DisjunctiveGraph.Disjunctions.ToList();
            var remainingConjunctions = Context.DisjunctiveGraph.Conjunctions.ToList();


            while (remainingNodes.Any())
            {
                //todo: o source está voltando para o allowed nodes pois ele não tem predecessores, logo nunca terá predecessores em remainingNodes
                var allowedNodes = GetAllowedNodes(remainingNodes);
                var feasibleMoves = GetFeasibleMoves(allowedNodes);
                if (feasibleMoves.IsEmpty())
                    break;

                var selectedMove = ChooseNextMove(feasibleMoves);

                ConjunctiveGraph.AddConjunctionAndVertices(selectedMove);

                var node = selectedMove.Target;
                var machine = selectedMove.Machine;
                
                LoadingSequence[machine].Push(node);

                var operationJobPredecessor = selectedMove.Source;
                var operationMachinePredecessor = LoadingSequence[machine].Peek();

                double jobCompletionTime = CompletionTimes[operationJobPredecessor.Operation];
                double machineCompletionTime = CompletionTimes[operationMachinePredecessor.Operation];
                
                CompletionTimes[node.Operation] = Math.Max(machineCompletionTime, jobCompletionTime) + selectedMove.Weight;

                RemoverArestas(remainingDisjunctions, remainingConjunctions, selectedMove);
                remainingNodes.Remove(node);

            }

            foreach(var machine in LoadingSequence.Keys)
            {
                Console.Write($"{machine.Id}: ");
                foreach (var node in LoadingSequence[machine].Reverse())
                    Console.Write($" {node}[{CompletionTimes[node.Operation]}] ");

                Console.WriteLine("");
            }

            UpdatePheromone();
        }

        private static void RemoverArestas(List<Disjunction> remainingDisjunctions, List<Conjunction> remainingConjunctions, Conjunction selectedMove)
        {
            if (selectedMove.HasAssociatedDisjunction)
                remainingDisjunctions.RemoveAll(d => d.IsAdjacent(selectedMove.Source) && d.IsAdjacent(selectedMove.Target));
            else
                remainingConjunctions.RemoveAll(c => c.Source == selectedMove.Source && c.Target == selectedMove.Target);
        }

        private Conjunction ChooseNextMove(IEnumerable<IFeasibleMove> possibleMoves)
        {
            var roulette = new RouletteWheelSelection<Conjunction>();
            // calculate the product tauK^ALPHA * etaK ^ BETA of the ant k for each edge
            possibleMoves.ToList().ForEach(move =>
            {
                var pheromoneAmount = move.PheromoneAmount;
                // assume that x = edge.Start and y = edge.Target
                var tauK_xy = move.Weight.Inverse(); // inverso da distancia entre edge.Start e edge.Target
                var etaK_xy = pheromoneAmount.DividedBy(move.Weight);// concentração de feromonio entre edge.Start e edge.Target
                var factor = Math.Pow(tauK_xy, Context.Alpha) * Math.Pow(etaK_xy, Context.Beta);
                roulette.AddItem(move.DirectedEdge, factor);
            });

            // calculate the probability pK of the ant k choosing each edge
            // then we draw one based on probability
            return roulette.SelectItem();
        }

        private void UpdatePheromone()
        {
            foreach (var edge in ConjunctiveGraph.Edges)
            {
                edge.EvaporatePheromone(rate: Context.Rho);
                var amount = Context.Q.DividedBy(Makespan);
                edge.DepositPheromone(amount);

            }
        }

        private Func<Node, bool> DoesNotContainPredecessorsIn(List<Node> remainingNodes)
        {
            return v => {
                var predecessors = Context.DisjunctiveGraph.GetPredecessors(v);
                return predecessors.Intersect(remainingNodes).IsEmpty();
            };
        }


        private List<Node> GetAllowedNodes(List<Node> remainingNodes) {
            if (remainingNodes.Count == 1 && remainingNodes.First().IsSinkNode)
                return remainingNodes;

            //return Context.DisjunctiveGraph.OperationVertices
            return remainingNodes
                    .Where(DoesNotContainPredecessorsIn(remainingNodes))
                    .ToList();
        }

        /// <summary>
        /// At a given iteration, operations that are candidates to be handled are those 
        /// that are still unhandled and such that do not have unhandled predecessors.
        /// </summary>
        /// <param name="allowedNodes"></param>
        /// <returns></returns>
        private List<Node> GetCandidateNodes(List<Node> allowedNodes)
        {
            Dictionary<Node, double> meanProcessingTime = new();
            foreach (var node in Context.DisjunctiveGraph.OperationVertices)
            {
                var meanP_i = node.Operation.EligibleMachines.Average(node.Operation.GetProcessingTime);
                meanProcessingTime.Add(node, meanP_i);
            }
            //
            return allowedNodes;
        }

        /// <summary>
        /// mark as feasible move 
        /// each disjunctive arc which connects a candidate operation to the last operation of the loading sequence 
        /// denoted by the same kind of arc(it makes the possibility 
        /// that the candidate operation becomes the new last 
        /// operation of that loading sequence);
        /// </summary>
        /// <param name="candidateNodes"></param>
        /// <returns></returns>
        private IEnumerable<IFeasibleMove> GetFeasibleMoves(List<Node> candidateNodes)
        {

            // only disjunctions adjascents to last operations on loading sequence and a candidate node
            var disjunctiveMoves = candidateNodes.SelectMany(candidateNode =>
            {
                return LoadingSequence.Values.SelectMany(sequence =>
                    Context.DisjunctiveGraph.Disjunctions
                        .Where(disjunction => disjunction.IsAdjacent(sequence.Peek()) && disjunction.IsAdjacent(candidateNode))//disjunctions adjascents to last operations
                        //.Where(disjunction => !ConjunctiveGraph.ContainsEdge(disjunction.Source, disjunction.Target) 
                        //                        && !ConjunctiveGraph.ContainsEdge(disjunction.Target, disjunction.Source))// not directed yet
                        .Select(disjunction => {
                            var direction = disjunction.Target == candidateNode ? Direction.SourceToTarget : Direction.TargetToSource;
                            return new DisjunctiveFeasibleMove(disjunction, direction);
                        })
                );
            });


            //TODO: Olhar para as conjunções que saiam de alguem no topo da pilha para alguem no candidateNodes

            var conjunctiveMoves = LoadingSequence.Values.SelectMany(machine =>
                Context.DisjunctiveGraph.GetDirectSuccessors(machine.Peek())
                .Where(c => candidateNodes.Contains(c.Target))
                .Select(conjunction => new ConjunctiveFeasibleMove(conjunction))
            );

            return [..disjunctiveMoves, ..conjunctiveMoves];
        }

    }
}
