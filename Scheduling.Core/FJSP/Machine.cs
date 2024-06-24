using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduling.Core.Models
{
    public class Machine
    {
        public Machine(int id, int machinePoolId = 0)
        {
            Id = id;
            // job shop is a special case of flexible job shop (when every machine pool has only 1 machine)
            MachinePoolId = machinePoolId != 0 ? machinePoolId : id;
        }

        public int Id { get; set; }

        public int MachinePoolId { get; set; }
    }
}
