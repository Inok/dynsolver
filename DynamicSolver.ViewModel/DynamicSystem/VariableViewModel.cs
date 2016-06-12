using System;
using JetBrains.Annotations;
using ReactiveUI;

namespace DynamicSolver.ViewModel.DynamicSystem
{
    public class VariableViewModel : ReactiveObject
    {
        public string Name { get; }
 
        private double? _value;
        public double? Value
        {
            get { return _value; }
            set { this.RaiseAndSetIfChanged(ref _value, value); }
        }
 
        public VariableViewModel([NotNull] string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
 
            Name = name;
        }
    }
}