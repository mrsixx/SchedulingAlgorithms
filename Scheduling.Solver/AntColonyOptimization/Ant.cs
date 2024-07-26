using QuikGraph;
using Scheduling.Core.Extensions;
using Scheduling.Core.Graph;
using Scheduling.Core.Interfaces;
using Scheduling.Solver.Extensions;
using Scheduling.Solver.Utils;
using System.Linq;

namespace Scheduling.Solver.AntColonyOptimization
{
    internal class Ant
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

        public List<Conjunction> Path { get; } = [];

        public ConjunctiveGraphModel ConjunctiveGraph { get; } = new ConjunctiveGraphModel();

        public Node StartNode => Context.DisjunctiveGraph.Source;

        public Node FinalNode => Context.DisjunctiveGraph.Sink;

        public double PathDistance => Path.CalculateDistance();


        public AntColonyOptimizationAlgorithmSolver Context { get; }

        public Task Task { get; }

        private void WalkAround()
        {
            // builds a conjunctive graph model (DAG which represents a feasible schedule) by the list scheduler algorithm
            ConjunctiveGraph.AddVertex(StartNode);
            foreach (var node in Context.DisjunctiveGraph.OperationVertices) {
                var operation = node.Operation;
                var allowedNodes = GetAllowedNodes();
            }
            ConjunctiveGraph.AddVertex(FinalNode);
            //var currentNode = StartNode;
            //while (currentNode != FinalNode)
            //{
            //    if (!Context.DisjunctiveGraph.TryGetDisjunctiveGraphOutEdges(currentNode, out IEnumerable<BaseEdge> currentNodeEdges))
            //        break;

            //    IEnumerable<Conjunction> possibleEdges = ExtractPossibleEdges(currentNode, currentNodeEdges);

            //    if (possibleEdges.IsEmpty())
            //        break;

            //    var selectedEdge = ChooseNextStep(possibleEdges);
            //    Path.Add(selectedEdge);
            //    currentNode = selectedEdge.Target;
            //}

            //if (currentNode == FinalNode)
            //    UpdatePheromone();
        }

        private IEnumerable<Conjunction> ExtractPossibleEdges(Node currentNode, IEnumerable<BaseEdge> currentNodeEdges)
        {
            return currentNodeEdges.Aggregate(new List<Conjunction>(), (acc, e) =>
                {
                    if (e is Conjunction c)
                        acc.Add(c);
                    if (e is Disjunction d)
                        acc.AddRange(d.EquivalentConjunctions);
                    return acc;
                })
                .Where(arc => arc.Source == currentNode)
                .ToList();
        }

        private Conjunction ChooseNextStep(IEnumerable<Conjunction> possibleEdges)
        {
            var roulette = new RouletteWheelSelection<Conjunction>();
            // calculate the product tauK^ALPHA * etaK ^ BETA of the ant k for each edge
            possibleEdges.ToList().ForEach(edge =>
            {
                // assume that x = edge.Start and y = edge.Target
                var tauK_xy = edge.Weight.Inverse(); // inverso da distancia entre edge.Start e edge.Target
                var etaK_xy = edge.Pheromone.DividedBy(edge.Weight);// concentração de feromonio entre edge.Start e edge.Target
                var factor = Math.Pow(tauK_xy, Context.Alpha) * Math.Pow(etaK_xy, Context.Beta);
                roulette.AddItem(edge, factor);
            });

            // calculate the probability pK of the ant k choosing each edge
            // then we draw one based on probability
            return roulette.SelectItem();
        }

        private void UpdatePheromone()
        {
            foreach (var step in Path)
            {
                step.EvaporatePheromone(rate: Context.Rho);
                step.DepositPheromone(amount: Context.Q.DividedBy(PathDistance));
            }
        }

        private List<Node> GetAllowedNodes() => Context.DisjunctiveGraph.OperationVertices.Where(ConjunctiveGraph.Vertices.DoesNotContain)
                                                                                          .ToList();
    }
}
