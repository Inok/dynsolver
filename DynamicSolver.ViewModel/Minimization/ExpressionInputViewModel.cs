using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DynamicSolver.Abstractions.Expression;
using DynamicSolver.ExpressionParser.Parser;
using DynamicSolver.ViewModel.Annotations;
using ReactiveUI;

namespace DynamicSolver.ViewModel.Minimization
{
    public class ExpressionInputViewModel : ReactiveObject
    {
        private readonly IExpressionParser _parser;

        private string _expression;
        public string Expression
        {
            get { return _expression; }
            set { this.RaiseAndSetIfChanged(ref _expression, value); }
        }

        private readonly ObservableAsPropertyHelper<IReactiveList<VariableViewModel>> _variables;
        public IReactiveList<VariableViewModel> Variables => _variables.Value;


        private readonly ObservableAsPropertyHelper<ParseResultViewModel> _parseResult;
        public ParseResultViewModel ParseResult => _parseResult.Value;

        private readonly ObservableAsPropertyHelper<MinimizationTaskInput> _taskInput;
        public MinimizationTaskInput MinimizationInput  => _taskInput.Value;


        public ExpressionInputViewModel([NotNull] IExpressionParser parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            _parser = parser;

            _parseResult = this.WhenAnyValue(m => m.Expression)
                .Select(GetParsingResult)
                .ToProperty(this, m => m.ParseResult, new ParseResultViewModel(null, true, null));

            _variables = this.WhenAnyValue(m => m.ParseResult)
                .Throttle(TimeSpan.FromSeconds(0.5), DispatcherScheduler.Current)
                .Where(m => m.Valid)
                .Select(m => m.Statement)
                .Select(CreateVariables).ToProperty(this, m => m.Variables, new ReactiveList<VariableViewModel>());

            _taskInput = Observable.Merge(
                this.WhenAnyValue(m => m.ParseResult).Select(_ => Unit.Default),
                this.WhenAnyValue(m => m.Variables).Select(_ => Unit.Default),
                this.WhenAnyObservable(m => m.Variables.ItemChanged).Select(_ => Unit.Default))
                .Select(_ => GetTaskInput())
                .ToProperty(this, m => m.MinimizationInput);
        }

        private MinimizationTaskInput GetTaskInput()
        {
            if (ParseResult.Statement == null)
            {
                return null;
            }

            if (Variables.Any(v => !v.Value.HasValue))
            {
                return null;
            }

            if (!ParseResult.Statement.Analyzer.GetVariablesSet().SetEquals(Variables.Select(v => v.VariableName)))
            {
                return null;
            }

            return new MinimizationTaskInput(ParseResult.Statement, Variables);
        }

        private IReactiveList<VariableViewModel> CreateVariables(IStatement statement)
        {
            if (statement == null)
            {
                return new ReactiveList<VariableViewModel>();
            }

            var expressionVariables = statement.Analyzer.Variables;

            var comparer = Comparer<VariableViewModel>.Create((v1, v2) => string.Compare(v1.VariableName, v2.VariableName, StringComparison.Ordinal));
            var newVariables = new SortedSet<VariableViewModel>(comparer);

            foreach (var model in Variables.Where(v => expressionVariables.Contains(v.VariableName)))
            {
                newVariables.Add(new VariableViewModel(model.VariableName) {Value = model.Value});
            }

            foreach (var variable in expressionVariables.Select(s => new VariableViewModel(s)))
            {
                newVariables.Add(variable);
            }

            return new ReactiveList<VariableViewModel>(newVariables) { ChangeTrackingEnabled = true };
        }

        private ParseResultViewModel GetParsingResult(string expression)
        {
            if(string.IsNullOrEmpty(expression))
            {
                return new ParseResultViewModel(null, true, null);
            }

            try
            {
                return new ParseResultViewModel(_parser.Parse(expression), true, null);
            }
            catch (FormatException e)
            {
                return new ParseResultViewModel(null, false, e.Message);
            }
        }
    }
}