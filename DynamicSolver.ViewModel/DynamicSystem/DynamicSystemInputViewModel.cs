using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.DynamicSystem;
using DynamicSolver.Expressions.Parser;
using JetBrains.Annotations;
using ReactiveUI;

namespace DynamicSolver.ViewModel.DynamicSystem
{
    public class ExplicitOrdinaryDifferentialEquationSystemViewModel : ReactiveObject
    {
        private readonly IExpressionParser _parser;

        private string _input;
        private ExplicitOrdinaryDifferentialEquationSystem _equationSystem;

        [CanBeNull]
        public string Input
        {
            get { return _input; }
            set { this.RaiseAndSetIfChanged(ref _input, value); }
        }

        [NotNull, ItemNotNull]
        public IReactiveList<string> Errors { get; }

        [CanBeNull]
        public ExplicitOrdinaryDifferentialEquationSystem EquationSystem
        {
            get { return _equationSystem; }
            set { this.RaiseAndSetIfChanged(ref _equationSystem, value); }
        }

        public ExplicitOrdinaryDifferentialEquationSystemViewModel([NotNull] IExpressionParser parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            _parser = parser;

            Errors = new ReactiveList<string>();
            
            this.WhenAnyValue(x => x.Input).Subscribe(ParseInput);
        }

        private void ParseInput(string userInput)
        {
            Errors.Clear();
            EquationSystem = null;

            var lines = (userInput ?? string.Empty).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var equations = new List<ExplicitOrdinaryDifferentialEquation>();
            foreach (var pair in lines.Select((s, i) => new { line = s, number = i }))
            {
                try
                {
                    equations.Add(ExplicitOrdinaryDifferentialEquation.FromStatement(_parser.Parse(pair.line)));
                }
                catch (FormatException e)
                {
                    Errors.Add($"{pair.number + 1} statement: {e.Message}");
                }
            }

            if (Errors.Count > 0 || equations.Count == 0)
            {
                return;
            }

            try
            {
                EquationSystem = new ExplicitOrdinaryDifferentialEquationSystem(equations);                
            }
            catch (FormatException e)
            {
                Errors.Add(e.Message);                
            }
        }

    }
}