using System;
using System.Diagnostics;

namespace DynamicSolver.DynamicSystem.Tests.Experiments
{
    public class ExecutionTimeAnalyzer : IDisposable
    {
        private readonly string _title;
        private readonly Stopwatch _sw;
        private int _iterationCount;

        public ExecutionTimeAnalyzer(string title = null)
        {
            _title = title ?? "Elapsed time";
            _iterationCount = 0;
            _sw = new Stopwatch();
            _sw.Start();
        }

        public void StartIteration()
        {
            _iterationCount++;
        }

        public void Dispose()
        {
            _sw.Stop();

            var elapsed = TimeSpan.FromTicks(_sw.Elapsed.Ticks / _iterationCount);

            Console.WriteLine($"{_title}: avg. of {_iterationCount, 2} runs is {elapsed} ({elapsed.Ticks} ticks)");
        }
    }
}