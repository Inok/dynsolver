using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace DynamicSolver.ViewModel.DynamicSystem
{
    public class VariableValue : INotifyPropertyChanged
    {
        public string VariableName { get; }
 
        private double? _value;
        public double? Value
        {
            get { return _value; }
            set
            {
                if (value.Equals(_value)) return;
                _value = value;
                OnPropertyChanged();
            }
        }
 
        public VariableValue([NotNull] string variableName)
        {
            if (variableName == null) throw new ArgumentNullException(nameof(variableName));
 
            VariableName = variableName;
        }
 
        public event PropertyChangedEventHandler PropertyChanged;
 
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}