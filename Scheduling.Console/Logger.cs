using Scheduling.Core.Interfaces;

namespace Scheduling.Console
{
    internal class Logger : ILogger
    {
        public void Log(string message) => System.Console.WriteLine(message);
    }
}
