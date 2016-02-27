using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DynamicSolver.Abstractions.Expression;
using DynamicSolver.ExpressionParser.Parser;
using DynamicSolver.ExpressionParser.Tools;
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

        private readonly ReactiveList<VariableViewModel> _variables = new ReactiveList<VariableViewModel>();
        public IReadOnlyReactiveList<VariableViewModel> Variables => _variables;


        private readonly ObservableAsPropertyHelper<ParseResultViewModel> _parseResult;
        public ParseResultViewModel ParseResult => _parseResult.Value;

        public ExpressionInputViewModel([NotNull] IExpressionParser parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            _parser = parser;

            _parseResult = this.WhenAnyValue(m => m.Expression)
                .Select(GetParsingResult)
                .ToProperty(this, m => m.ParseResult, new ParseResultViewModel(null, true, null));

            this.WhenAnyValue(m => m.ParseResult)
                .Throttle(TimeSpan.FromSeconds(0.5), DispatcherScheduler.Current)
                .Where(m => m.Valid)
                .Select(m => m.Statement?.Expression)
                .Subscribe(UpdateVariables);
        }

        private void UpdateVariables(IExpression expr)
        {
            if (expr == null)
            {
                _variables.Clear();
                return;
            }

            var set = new HashSet<string>();

            var visitor = new ExpressionVisitor(expr);
            visitor.VisitVariablePrimitive += (_, v) => set.Add(v.Name);
            visitor.Visit();

            _variables.RemoveAll(_variables.Where(v => !set.Contains(v.VariableName)).ToList());

            foreach (var variable in _variables)
            {
                set.Remove(variable.VariableName);
            }

            foreach (var variable in set.Select(s => new VariableViewModel(s)))
            {
                _variables.Add(variable);
            }

            _variables.Sort((v1, v2) => string.Compare(v1.VariableName, v2.VariableName, StringComparison.Ordinal));
        }

        private ParseResultViewModel GetParsingResult(string expression)
        {
            if(string.IsNullOrEmpty(expression))
                return new ParseResultViewModel(null, true, null);
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