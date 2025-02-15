using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scheduling.Core.FJSP;

namespace Scheduling.Benchmarks.Interfaces
{
    public interface IBenchmarkReaderService
    {
        Instance ReadInstance(string file);
    }
}
