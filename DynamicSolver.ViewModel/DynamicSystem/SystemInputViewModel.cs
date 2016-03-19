using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DynamicSolver.DynamicSystem;
using DynamicSolver.ExpressionParser.Parser;
using DynamicSolver.Minimizer;
using DynamicSolver.ViewModel.Annotations;
using ReactiveUI;

namespace DynamicSolver.ViewModel.DynamicSystem
{
    public class SystemInputViewModel : ReactiveObject
    {
        private readonly IExpressionParser _parser;

        private string _expression;
        private double _step = 0.01;
        private double _time = 1;

        private readonly ObservableAsPropertyHelper<ExplicitOrdinaryDifferentialEquationSystem> _system;
        private readonly ObservableAsPropertyHelper<string> _errorMessage;
        private readonly ObservableAsPropertyHelper<IReactiveList<VariableValue>> _variables;
        private readonly ObservableAsPropertyHelper<DynamicSystemSolverInput> _taskInput;
        
        public string Expression
        {
            get { return _expression; }
            set { this.RaiseAndSetIfChanged(ref _expression, value); }
        }

        public double Step
        {
            get { return _step; }
            set { this.RaiseAndSetIfChanged(ref _step, value); }
        }

        public double Time
        {
            get { return _time; }
            set { this.RaiseAndSetIfChanged(ref _time, value); }
        }

        public ExplicitOrdinaryDifferentialEquationSystem System => _system.Value;
        public string ErrorMessage => _errorMessage.Value;
        public IReactiveList<VariableValue> Variables => _variables.Value;
        public DynamicSystemSolverInput TaskInput  => _taskInput.Value;
        

        public SystemInputViewModel([NotNull] IExpressionParser parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            _parser = parser;

            var parseResult = this.WhenAnyValue(m => m.Expression).Select(GetParsingResult);

            _system = parseResult.Select(m => m.Item1).ToProperty(this, m => m.System);
            _errorMessage = parseResult.Select(p => string.Join(Environment.NewLine, p.Item2)).ToProperty(this, m => m.ErrorMessage);

            _variables = parseResult
                .Where(m => m.Item2.Length == 0)
                .Throttle(TimeSpan.FromSeconds(0.5), DispatcherScheduler.Current)
                .Select(m => CreateVariables(m.Item1))
                .ToProperty(this, m => m.Variables, new ReactiveList<VariableValue>());

            _taskInput = Observable.Merge(
                parseResult.Select(_ => Unit.Default),
                this.WhenAnyValue(m => m.Time).Select(_ => Unit.Default),
                this.WhenAnyValue(m => m.Step).Select(_ => Unit.Default),
                this.WhenAnyValue(m => m.Variables).Select(_ => Unit.Default),
                this.WhenAnyObservable(m => m.Variables.ItemChanged).Select(_ => Unit.Default))
                .Throttle(TimeSpan.FromSeconds(0.5), DispatcherScheduler.Current)
                .Select(_ => GetTaskInput())
                .ToProperty(this, m => m.TaskInput);            
        }

        private IReactiveList<VariableValue> CreateVariables(ExplicitOrdinaryDifferentialEquationSystem system)
        {
            if (system == null)
            {
                return new ReactiveList<VariableValue>();
            }

            var expressionVariables = system.Equations.SelectMany(e => e.Function.Analyzer.Variables)
                .Concat(system.Equations.Select(e => e.LeadingDerivative.Variable.Name))
                .Distinct().ToList();

            var modellingVariable = system.Equations.Select(e => e.LeadingDerivative.Variable.Name).Any(v => v == "t") ? string.Empty : "t";
            if (!expressionVariables.Contains(modellingVariable))
            {
                var independentVariables = expressionVariables.Except(system.Equations.Select(e => e.LeadingDerivative.Variable.Name)).ToList();
                if (independentVariables.Count == 1)
                {
                    modellingVariable = independentVariables[0];
                }
            }

            expressionVariables.Remove(modellingVariable);

            var comparer = Comparer<VariableValue>.Create((v1, v2) => string.Compare(v1.VariableName, v2.VariableName, StringComparison.Ordinal));
            var newVariables = new SortedSet<VariableValue>(comparer);

            foreach (var model in Variables.Where(v => expressionVariables.Contains(v.VariableName)))
            {
                newVariables.Add(new VariableValue(model.VariableName) {Value = model.Value});
            }

            foreach (var variable in expressionVariables.Select(s => new VariableValue(s)))
            {
                newVariables.Add(variable);
            }

            return new ReactiveList<VariableValue>(newVariables) { ChangeTrackingEnabled = true };
        }

        private DynamicSystemSolverInput GetTaskInput()
        {
            if (System == null)
            {
                return null;
            }

            if (Variables.Any(v => !v.Value.HasValue))
            {
                return null;
            }

            return new DynamicSystemSolverInput(System, Variables, Step, Time);
        }

        private Tuple<ExplicitOrdinaryDifferentialEquationSystem, string[]> GetParsingResult(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                return new Tuple<ExplicitOrdinaryDifferentialEquationSystem, string[]>(null, new string[0]);
            }

            var equations = new List<ExplicitOrdinaryDifferentialEquation>();
            var errors = new List<string>();
            foreach (var pair in expression.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).Select((s, i) => new {s, i}))
            {
                try
                {
                    var statement = _parser.Parse(pair.s);
                    equations.Add(ExplicitOrdinaryDifferentialEquation.FromStatement(statement));
                }
                catch (FormatException e)
                {
                    errors.Add(pair.i + " line: " + e.Message);
                }
            }

            if (errors.Count > 0 || equations.Count == 0)
            {
                return new Tuple<ExplicitOrdinaryDifferentialEquationSystem, string[]>(null, errors.ToArray());
            }

            try
            {
                var system = new ExplicitOrdinaryDifferentialEquationSystem(equations);
                return new Tuple<ExplicitOrdinaryDifferentialEquationSystem, string[]>(system, new string[0]);
            }
            catch (FormatException e)
            {
                return new Tuple<ExplicitOrdinaryDifferentialEquationSystem, string[]>(null, new[] {e.Message});
            }

        }
    }
}