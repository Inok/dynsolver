using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Core.Syntax.Parser;
using DynamicSolver.Modelling;
using DynamicSolver.ViewModel.Common.ErrorList;
using JetBrains.Annotations;
using ReactiveUI;

namespace DynamicSolver.ViewModel.DynamicSystem
{
    public class ExplicitOrdinaryDifferentialEquationSystemViewModel : ReactiveObject
    {
        private readonly ISyntaxParser _parser;

        private string _input;
        private IReadOnlyCollection<ExplicitOrdinaryDifferentialEquation> _equationSystem;

        [CanBeNull]
        public string Input
        {
            get { return _input; }
            set { this.RaiseAndSetIfChanged(ref _input, value); }
        }

        [NotNull]
        public ErrorListViewModel ErrorListViewModel { get; }

        [CanBeNull]
        public IReadOnlyCollection<ExplicitOrdinaryDifferentialEquation> EquationSystem
        {
            get { return _equationSystem; }
            set { this.RaiseAndSetIfChanged(ref _equationSystem, value); }
        }

        public ExplicitOrdinaryDifferentialEquationSystemViewModel([NotNull] ISyntaxParser parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            _parser = parser;

            ErrorListViewModel = new ErrorListViewModel();
            
            this.WhenAnyValue(x => x.Input).Subscribe(ParseInput);
        }

        private void ParseInput(string userInput)
        {
            ErrorListViewModel.Errors.Clear();

            EquationSystem = null;

            var lines = (userInput ?? string.Empty).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var equations = new List<ExplicitOrdinaryDifferentialEquation>();
            foreach (var pair in lines.Select((s, i) => new { line = s, number = i }))
            {
                try
                {
                    equations.Add(ExplicitOrdinaryDifferentialEquation.FromExpression(_parser.Parse(pair.line)));
                }
                catch (FormatException e)
                {
                    ErrorListViewModel.Errors.Add(new ErrorViewModel()
                    {
                        Level = ErrorLevel.Error,
                        Source = $"{pair.number + 1} statement",
                        Message = e.Message
                    });
                }
            }

            if (ErrorListViewModel.HasErrors)
            {
                return;
            }

            EquationSystem = equations;
        }

    }
}