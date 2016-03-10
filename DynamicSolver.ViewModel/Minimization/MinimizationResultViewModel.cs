﻿using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Minimizer;
using DynamicSolver.ViewModel.Annotations;
using ReactiveUI;

namespace DynamicSolver.ViewModel.Minimization
{
    public class MinimizationResultViewModel : ReactiveObject
    {
        private IReadOnlyCollection<VariableValue> _variables;
        private bool _isInProgress;
        private string _method;
        private string _errorMessage;

        public MinimizationResultViewModel(string method)
        {
            _method = method;
        }

        public IReadOnlyCollection<VariableValue> Variables
        {
            get { return _variables; }
            private set { this.RaiseAndSetIfChanged(ref _variables, value); }
        }

        public bool IsInProgress
        {
            get { return _isInProgress; }
            private set { this.RaiseAndSetIfChanged(ref _isInProgress, value); }
        }

        public string Method
        {
            get { return _method; }
            private set { this.RaiseAndSetIfChanged(ref _method, value); }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            private set { this.RaiseAndSetIfChanged(ref _errorMessage, value); }
        }

        public void StartProgress()
        {
            IsInProgress = true;
            Variables = new VariableValue[0];
        }

        public void ApplyResult(IEnumerable<VariableValue> result)
        {
            Variables = result.OrderBy(v => v.VariableName).ToList();
            ErrorMessage = null;
            IsInProgress = false;
        }

        public void ApplyFail([NotNull] string error)
        {
            if (string.IsNullOrEmpty(error)) throw new ArgumentException("Value cannot be null or empty.", nameof(error));

            ErrorMessage = error;
            IsInProgress = false;
        }

        public void Clear()
        {
            Variables = new VariableValue[0];
            ErrorMessage = null;
            IsInProgress = false;
        }
    }
}