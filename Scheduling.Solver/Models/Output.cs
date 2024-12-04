using Newtonsoft.Json;
using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization;

namespace Scheduling.Solver.Models
{
    public class Output
    {

        [JsonProperty("resources")]
        public List<OutputMachine> Machines { get; } = [];


        [JsonProperty("events")]
        public List<OutputEvent> Events { get; } = [];


        [JsonProperty("assignments")]
        public List<OutputAssignment> Assignments { get; } = [];
    }

    public class OutputEvent
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name => $"{Id}";

        [JsonProperty("startDate")]
        public double StartDate { get; set; }

        [JsonProperty("endDate")]
        public double EndDate { get; set; }

        [JsonProperty("eventColor")]
        public string Color { get; set; } = "orange";

        public static OutputEvent FromNode(Node node, Ant ant)
        {
            return new OutputEvent
            {
                Id = node.Id,
                StartDate = ant.StartTimes[node.Operation],
                EndDate = ant.CompletionTimes[node.Operation],
            };
        }
    }

    public class  OutputMachine(int id)
    {

        [JsonProperty("id")]
        public int Id { get; set; } = id;

        [JsonProperty("name")]
        public string Name => $"M{Id}";

        [JsonProperty("iconCls")]
        public string Icon => "b-fa b-fa-print";
    }

    public class OutputAssignment
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("eventId")]
        public int EventId { get; set; }

        [JsonProperty("resourceId")]
        public int MachineId { get; set; }
    }
}
