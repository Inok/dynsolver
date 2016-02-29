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
        private class ParseResult
        {
            public IStatement Statement { get; }
            public string ErrorMessage { get; }

            public ParseResult(IStatement statement, string errorMessage)
            {
                Statement = statement;
                ErrorMessage = errorMessage;
            }
        }

        private readonly IExpressionParser _parser;

        private string _expression;
        private readonly ObservableAsPropertyHelper<IStatement> _statement;
        private readonly ObservableAsPropertyHelper<string> _errorMessage;
        private readonly ObservableAsPropertyHelper<IReactiveList<VariableViewModel>> _variables;
        private readonly ObservableAsPropertyHelper<MinimizationTaskInput> _taskInput;

        public string Expression
        {
            get { return _expression; }
            set { this.RaiseAndSetIfChanged(ref _expression, value); }
        }

        public IStatement Statement => _statement.Value;
        public string ErrorMessage => _errorMessage.Value;
        public IReactiveList<VariableViewModel> Variables => _variables.Value;
        public MinimizationTaskInput MinimizationInput  => _taskInput.Value;


        public ExpressionInputViewModel([NotNull] IExpressionParser parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            _parser = parser;

            var parseResult = this.WhenAnyValue(m => m.Expression).Select(GetParsingResult);

            _statement = parseResult.Select(m => m.Statement).ToProperty(this, m => m.Statement);
            _errorMessage = parseResult.Select(p => p.ErrorMessage).ToProperty(this, m => m.ErrorMessage);

            _variables = parseResult
                .Where(m => m.ErrorMessage == null)
                .Throttle(TimeSpan.FromSeconds(0.5), DispatcherScheduler.Current)
                .Select(m => CreateVariables(m.Statement))
                .ToProperty(this, m => m.Variables, new ReactiveList<VariableViewModel>());

            _taskInput = Observable.Merge(
                parseResult.Select(_ => Unit.Default),
                this.WhenAnyValue(m => m.Variables).Select(_ => Unit.Default),
                this.WhenAnyObservable(m => m.Variables.ItemChanged).Select(_ => Unit.Default))
                .Select(_ => GetTaskInput())
                .ToProperty(this, m => m.MinimizationInput);
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

        private MinimizationTaskInput GetTaskInput()
        {
            if (Statement == null)
            {
                return null;
            }

            if (Variables.Any(v => !v.Value.HasValue))
            {
                return null;
            }

            if (!Statement.Analyzer.GetVariablesSet().SetEquals(Variables.Select(v => v.VariableName)))
            {
                return null;
            }

            return new MinimizationTaskInput(Statement, Variables);
        }

        private ParseResult GetParsingResult(string expression)
        {
            if(string.IsNullOrEmpty(expression))
            {
                return new ParseResult(null, null);
            }

            try
            {
                return new ParseResult(_parser.Parse(expression), null);
            }
            catch (FormatException e)
            {
                return new ParseResult(null, e.Message);
            }
        }
    }
}