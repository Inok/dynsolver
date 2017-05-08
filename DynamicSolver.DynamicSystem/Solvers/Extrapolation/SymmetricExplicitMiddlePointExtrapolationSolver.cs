using System.Linq;
using DynamicSolver.DynamicSystem.Solvers.Explicit;
using DynamicSolver.DynamicSystem.Step;

namespace DynamicSolver.DynamicSystem.Solvers.Extrapolation
{
    public class SymmetricExplicitMiddlePointExtrapolationSolver : ExtrapolationSolver
    {
        public SymmetricExplicitMiddlePointExtrapolationSolver(int extrapolationStages)
            : base(new ExplicitMiddlePointExtrapolationDynamicSystemSolver(), extrapolationStages)
        {
        }

        protected override int[] GetExtrapolationCoefficients(int extrapolationStages)
        {
            return Enumerable.Range(1, extrapolationStages).Select(c => c * 2).ToArray();
        }

        protected override DynamicSystemState MakeExtrapolationSteps(IExplicitOrdinaryDifferentialEquationSystem state,
            IndependentVariableStep step, int extrapolationCoefficient)
        {
            var stepSize = step.Delta / extrapolationCoefficient;
            var extrapolationStepsLastValue = BaseSolver.Solve(state, new FixedStepStrategy(stepSize)).Take(extrapolationCoefficient - 1).Last();
            return extrapolationStepsLastValue;
        }
    }
}