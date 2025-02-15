using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization.Solvers;
using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Solver.AntColonyOptimization.Ants
{
    public class ListSchedulingAcsAntV2(int id, int generation, AntColonySystemAlgorithmSolver context)
        : BaseAnt(id, generation, context)
    {

        public override void WalkAround()
        { 
            InitializeDataStructures();
            var grauDeEntradaConjuntivo = new Dictionary<Node, int>();

            Context.DisjunctiveGraph.Vertices.ToList().ForEach(
                node => grauDeEntradaConjuntivo.Add(node, node.IncidentConjunctions.Count)
            );
            //TODO: os sucessores da fonte deveriam começar com node.IncidentConjunctions.Count - 1;

            //lista de escalaveis
            HashSet<Node> PendingNodes = [.. Context.DisjunctiveGraph.Source.Successors];
            HashSet<Node> ScheduledNodes = [Context.DisjunctiveGraph.Source];
            while (PendingNodes.Any())
            {

                var proximoMovimento = SelecionaProximoMovimento(PendingNodes, ScheduledNodes);
                if (proximoMovimento is null)
                    break;
                //calcular starting time e preencher grafo conjuntivo
                //TODO: talvez devessemos incluir todas as conjunções incidentes sobre um vértice (pra jogar o sink na ultima posição da ordenação topológica)
                EvaluateCompletionTime(proximoMovimento.DirectedEdge);
                //atualizar trilha de feromonio
                LocalPheromoneUpdate(proximoMovimento.DirectedEdge);

                //tira o target dos escalaveis
                PendingNodes.Remove(proximoMovimento.DirectedEdge.Target);
                ScheduledNodes.Add(proximoMovimento.DirectedEdge.Target);
                //olhar para os sucessores do target e decrementar o contador deles em uma unidade
                //para cada um deles que foi pra 0,  adiciona na lista de escalaveis
                proximoMovimento.DirectedEdge.Target.Successors.ForEach(node =>
                {
                    if (grauDeEntradaConjuntivo[node] <= 0) return;
                    grauDeEntradaConjuntivo[node] -= 1;
                    if (grauDeEntradaConjuntivo[node] == 0)
                        PendingNodes.Add(node);
                });
            }
        }

        private IFeasibleMove SelecionaProximoMovimento(HashSet<Node> escalaveis, HashSet<Node> escalados)
        {
            //olhar apenas para os arcos disjuntivos que tem uma ponta nos escalaveis e outra nos escalados
            //regra de pseudoprob

            var feasibleMoves = escalaveis.SelectMany(candidateNode =>
            {
                List<IFeasibleMove> moves = [];
                foreach (var disj in candidateNode.IncidentDisjunctions)
                {
                    if (escalados.Contains(disj.Other(candidateNode)))
                    {
                        var direction = disj.Target == candidateNode
                            ? Direction.SourceToTarget
                            : Direction.TargetToSource;
                        var move = new FeasibleMove(disj, direction);
                        moves.Add(move);
                    }
                }

                return moves;
            });
            //return feasibleMoves.First();
            return ChooseNextMove(feasibleMoves); // <---- GARGALO
        }

        private void LocalPheromoneUpdate(Orientation selectedMove)
        {
            if (!Context.PheromoneTrail.TryGetValue(selectedMove, out double currentPheromoneValue) || !Context.PheromoneTrail.TryUpdate(selectedMove, (1 - context.Phi) * currentPheromoneValue + context.Phi * context.Tau0, currentPheromoneValue))
                Console.WriteLine("Unable to decay pheromone after construction step...");
        }



        private void EvaluateCompletionTime(Orientation selectedMove)
        {
            var node = selectedMove.Target;
            var machine = selectedMove.Machine;
            EvaluateCompletionTime(node, machine);
            ConjunctiveGraph.AddConjunctionAndVertices(selectedMove);
        }

        private void EvaluateCompletionTime(Node node, Machine machine)
        {

            var jobPredecessorNode = node.DirectPredecessor;
            var machinePredecessorNode = LoadingSequence[machine].Peek();
            ConjunctiveGraph.AddConjunctionAndVertices(new Conjunction(jobPredecessorNode, node));
            var jobCompletionTime = CompletionTimes[jobPredecessorNode.Operation];
            var machineCompletionTime = CompletionTimes[machinePredecessorNode.Operation];

            var processingTime = node.Operation.GetProcessingTime(machine);
            StartTimes[node.Operation] = Math.Max(machineCompletionTime, jobCompletionTime);
            CompletionTimes[node.Operation] = StartTimes[node.Operation] + processingTime;


            LoadingSequence[machine].Push(node);
            if (!MachineAssignment.TryAdd(node.Operation, machine))
                throw new Exception($"Machine already assigned to this operation");
        }

        private IFeasibleMove ChooseNextMove(IEnumerable<IFeasibleMove> feasibleMoves)
        {
            var sum = 0.0;
            var rouletteWheel = new List<(IFeasibleMove Move, double Probability)>();
            IFeasibleMove? greedyMove = null;
            var greedyFactor = double.MinValue;
            // create roulette wheel and evaluate greedy move for pseudorandom proportional rule at same time (in O(n))
            foreach (var move in feasibleMoves)
            {
                var tauXy = move.GetPheromoneAmount(context); // pheromone amount
                var etaXy = move.Weight.Inverse(); // heuristic information
                var tauXyAlpha = Math.Pow(tauXy, context.Alpha); // pheromone amount raised to power alpha
                var etaXyBeta = Math.Pow(etaXy, context.Beta); // heuristic information raised to power beta

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

    }
}
