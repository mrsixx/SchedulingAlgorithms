using QuikGraph;
using Scheduling.Core.Extensions;
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
            Task = new Task(() => WalkAround());
            Task.Start();
        }

        public int Id { get; }

        public int Generation { get; }

        public ConjunctiveGraphModel ConjunctiveGraph { get; } = new ConjunctiveGraphModel();

        public Node StartNode => Context.DisjunctiveGraph.Source;

        public Node FinalNode => Context.DisjunctiveGraph.Sink;

        public AntColonyOptimizationAlgorithmSolver Context { get; }

        public Task Task { get; }

        private void WalkAround()
        {
            // builds a conjunctive graph model (DAG which represents a feasible schedule) by the list scheduler algorithm
            ConjunctiveGraph.AddVertex(StartNode);
            ConjunctiveGraph.AddVertex(FinalNode);
            var remainingDisjunctions = Context.DisjunctiveGraph.Disjunctions.ToList();
            foreach (var node in Context.DisjunctiveGraph.OperationVertices)
            {
                ConjunctiveGraph.AddVertex(node);

                var allowedNodes = GetAllowedNodes();
                var candidateNodes = GetCandidateNodes(allowedNodes);
                var feasibleMoves = GetFeasibleMoves(candidateNodes);
                if (feasibleMoves.IsEmpty()) break;

                var selectedMove = ChooseNextMove(feasibleMoves);
                ConjunctiveGraph.AddConjunctionAndVertices(selectedMove);
                ConjunctiveGraph.CriticalPath.Clear();
                ConjunctiveGraph.CriticalPath.AddRange(DAGLongestPathAlgorithm.LongestPath(ConjunctiveGraph));
                remainingDisjunctions.RemoveAll(r => r.IsAdjacent(selectedMove.Source));
            }

            foreach (var conjunction in Context.DisjunctiveGraph.Conjunctions)
                if(!ConjunctiveGraph.ContainsEdge(conjunction.Source, conjunction.Target))
                    ConjunctiveGraph.AddConjunctionAndVertices(conjunction);
            //TODO: linkar com o ralo
            foreach (var disjunction in Context.DisjunctiveGraph.Disjunctions.Where(d => d.IsAdjacent(Context.DisjunctiveGraph.Sink)))
                foreach (var conjunction in disjunction.EquivalentConjunctions.Where(c => c.Target == Context.DisjunctiveGraph.Sink))
                    if (!ConjunctiveGraph.ContainsEdge(conjunction))
                        ConjunctiveGraph.AddEdge(conjunction);

            UpdatePheromone();
        }

        private Conjunction ChooseNextMove(IEnumerable<FeasibleMove> possibleMoves)
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
            foreach (var disjunction in Context.DisjunctiveGraph.Disjunctions)
                disjunction.EvaporatePheromone(rate: Context.Rho);
            foreach (var conjunction in ConjunctiveGraph.Edges)
                if (conjunction.HasAssociatedDisjunction)
                    conjunction.AssociatedDisjunction.DepositPheromone(amount: Context.Q.DividedBy(ConjunctiveGraph.Makespan), conjunction.ChoosenDirection.Value);

        }

        private List<Node> GetAllowedNodes() => Context.DisjunctiveGraph.OperationVertices.Where(ConjunctiveGraph.Vertices.DoesNotContain)
                                                                                          .ToList();

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
        private List<FeasibleMove> GetFeasibleMoves(List<Node> candidateNodes)
        {

            List<FeasibleMove> feasibleMoves = new();

            candidateNodes.ForEach(candidateNode =>
            {
                var adjascentDisjunctions = Context.DisjunctiveGraph.Disjunctions
                    .Where(d => d.IsAdjacent(candidateNode) && ConjunctiveGraph.Vertices.Any(v => d.IsAdjacent(v)))
                    .ToList();

                adjascentDisjunctions.ForEach(d =>
                {
                    //TODO: include only those conserve DAG
                    var direction = (Direction)Random.Shared.Next(0, 2);
                    feasibleMoves.Add(new(d, direction));
                });
            });
            return feasibleMoves;
        }

    }
}
