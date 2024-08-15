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

        public Node StartNode => Context.DisjunctiveGraph.Source;

        public Node FinalNode => Context.DisjunctiveGraph.Sink;

        public AntColonyOptimizationAlgorithmSolver Context { get; }

        public Task Task { get; }

        private void WalkAround()
        {

            var remainingNodes = Context.DisjunctiveGraph.Vertices.ToList();


            //var remainingNodes = Context.DisjunctiveGraph.OperationVertices.ToList();
            //foreach (var conjunction in Context.DisjunctiveGraph.GetSuccessors(Context.DisjunctiveGraph.Source))
            //{
            //    ConjunctiveGraph.AddConjunctionAndVertices(conjunction);
            //    var selectedOperation = conjunction.Target.Operation;
            //    var greedyMachine = conjunction.Target.Operation.EligibleMachines.MinBy(m => selectedOperation.ProcessingTime(m));
            //    var selectedOperationMachinePredecessor = LoadingSequence[greedyMachine].Peek();
            //    var machineCompletionTime = selectedOperationMachinePredecessor.Operation.CompletionTime + selectedOperation.ProcessingTime(greedyMachine);
            //    LoadingSequence[greedyMachine].Push(conjunction.Target);
            //    remainingNodes.Remove(conjunction.Source);
            //    remainingNodes.Remove(conjunction.Target);
            //}


            while (remainingNodes.Any())
            {
                //todo: preciso fazer com que o source só seja removido do remainingNodes quando todos os primeiros 
                var allowedNodes = GetAllowedNodes(remainingNodes);
                var feasibleMoves = GetFeasibleMoves(allowedNodes);
                if (feasibleMoves.IsEmpty())
                    break;
                var selectedMove = ChooseNextMove(feasibleMoves);

                ConjunctiveGraph.AddConjunctionAndVertices(selectedMove);

                var selectedNode = selectedMove.Target;
                var selectedMachine = selectedMove.Machine;
                var selectedOperation = selectedNode.Operation;
                var selectedOperationJobPredecessor = Context.DisjunctiveGraph.GetPrecessor(selectedNode);
                var jobCompletionTime = selectedOperationJobPredecessor != null ? selectedOperationJobPredecessor.Operation.CompletionTime : 0;
                double machineCompletionTime = 0;
                if (selectedMachine != null)
                {
                    var selectedOperationMachinePredecessor = LoadingSequence[selectedMachine].Peek();
                    machineCompletionTime = selectedOperationMachinePredecessor.Operation.CompletionTime + selectedOperation.ProcessingTime(selectedMachine);
                    LoadingSequence[selectedMachine].Push(selectedNode);
                }

                selectedOperation.CompletionTime = Math.Max(machineCompletionTime, jobCompletionTime);
                remainingNodes.Remove(selectedMove.Source);
                remainingNodes.Remove(selectedMove.Target);

            }
            //foreach (var node in ConjunctiveGraph.Sinks().ToList())
            //{

            //    ConjunctiveGraph.AddConjunctionAndVertices(Context.DisjunctiveGraph.GetSuccessors(node).First());
            //}

            //UpdatePheromone();
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
            foreach (var disjunction in Context.DisjunctiveGraph.Edges)
                disjunction.EvaporatePheromone(rate: Context.Rho);

            foreach (var conjunction in ConjunctiveGraph.Edges)
            {
                var amount = Context.Q.DividedBy(ConjunctiveGraph.Makespan);
                if (conjunction.HasAssociatedDisjunction)
                    conjunction.AssociatedDisjunction.DepositPheromone(amount, conjunction.ChoosenDirection.Value);
                else
                    conjunction.DepositPheromone(amount);
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
            return Context.DisjunctiveGraph.Vertices
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
            var moves = candidateNodes.SelectMany(candidateNode =>
            {
                return LoadingSequence.Values.SelectMany(sequence =>
                    Context.DisjunctiveGraph.Disjunctions
                        .Where(disjunction => disjunction.IsAdjacent(sequence.Peek()) && disjunction.IsAdjacent(candidateNode))//disjunctions adjascents to last operations
                        .Where(disjunction => !ConjunctiveGraph.ContainsEdge(disjunction.Source, disjunction.Target) 
                                                && !ConjunctiveGraph.ContainsEdge(disjunction.Target, disjunction.Source))// not directed yet
                        .Select(disjunction => {
                            var direction = disjunction.Target == candidateNode ? Direction.SourceToTarget : Direction.TargetToSource;
                            return new DisjunctiveFeasibleMove(disjunction, direction);
                        })
                );
            });

            //if (moves.Any())
            //    return moves;


            return [..moves, ..candidateNodes.SelectMany(candidateNode =>
                Context.DisjunctiveGraph.GetDirectSuccessors(candidateNode)
                    .Where(conjunction => LoadingSequence.DoesNotContainNode(conjunction.Target)) //excluo conjunções cujo o alvo 
                    .Select(conjunction => new ConjunctiveFeasibleMove(conjunction))
            )];
        }

    }
}
