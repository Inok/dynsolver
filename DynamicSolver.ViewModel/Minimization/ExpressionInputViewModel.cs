﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DynamicSolver.Abstractions.Expression;
using DynamicSolver.Expressions.Parser;
using DynamicSolver.Minimizer;
using DynamicSolver.ViewModel.Properties;
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
        private readonly ObservableAsPropertyHelper<IReactiveList<VariableValue>> _variables;
        private readonly ObservableAsPropertyHelper<MinimizationTaskInput> _taskInput;

        public string Expression
        {
            get { return _expression; }
            set { this.RaiseAndSetIfChanged(ref _expression, value); }
        }

        public IStatement Statement => _statement.Value;
        public string ErrorMessage => _errorMessage.Value;
        public IReactiveList<VariableValue> Variables => _variables.Value;
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
                .ToProperty(this, m => m.Variables, new ReactiveList<VariableValue>());

            _taskInput = Observable.Merge(
                parseResult.Select(_ => Unit.Default),
                this.WhenAnyValue(m => m.Variables).Select(_ => Unit.Default),
                this.WhenAnyObservable(m => m.Variables.ItemChanged).Select(_ => Unit.Default))
                .Select(_ => GetTaskInput())
                .ToProperty(this, m => m.MinimizationInput);
        }

        private IReactiveList<VariableValue> CreateVariables(IStatement statement)
        {
            if (statement == null)
            {
                return new ReactiveList<VariableValue>();
            }

            var expressionVariables = statement.Analyzer.Variables;

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

            if (!Statement.Analyzer.IsComputable)
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
                var statement = _parser.Parse(expression);

                return statement.Analyzer.IsComputable
                    ? new ParseResult(statement, null)
                    : new ParseResult(null, "Provided statement isn't computable.");
            }
            catch (FormatException e)
            {
                return new ParseResult(null, e.Message);
            }
        }
    }
}