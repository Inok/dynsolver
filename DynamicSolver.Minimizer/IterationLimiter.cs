using System;

namespace DynamicSolver.Minimizer
{
    public class IterationLimiter
    {
        private readonly IIterationLimitSettings _settings;

        public int Iteration { get; private set; }

        public bool IterationLimitReached => Iteration >= _settings.MaxIterationCount;
        public bool ShouldInterrupt => !_settings.AbortSearchOnIterationLimit && Iteration >= _settings.MaxIterationCount;
        
        public IterationLimiter(IIterationLimitSettings settings)
        {
            if(settings.MaxIterationCount <=0)
                throw new ArgumentException("Settings has negative iteration limit.");
            _settings = settings;
        }

        public void NextIteration()
        {
            if(IterationLimitReached)
                throw new InvalidOperationException($"Search was interrupted because iteration limit has been reached: {_settings.MaxIterationCount}.");
            Iteration++;
        }

        public bool TryNextIteration()
        {
            if (IterationLimitReached)
            {
                return false;
            }
            Iteration++;
            return true;
        }
   }
}