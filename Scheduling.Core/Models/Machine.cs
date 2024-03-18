using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduling.Core.Models
{
    public class Machine
    {
        public Machine(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
    }
}
