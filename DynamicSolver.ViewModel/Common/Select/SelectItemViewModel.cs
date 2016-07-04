using System;
using JetBrains.Annotations;
using ReactiveUI;

namespace DynamicSolver.ViewModel.Common.Select
{
    public class SelectItemViewModel<T> : ReactiveObject
    {
        private string _name;
        private T _value;
        
        public SelectItemViewModel([NotNull] string name, T value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            _name = name;
            _value = value;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                this.RaiseAndSetIfChanged(ref _name, value);
            }
        }

        public T Value
        {
            get { return _value; }
            set { this.RaiseAndSetIfChanged(ref _value, value); }
        }
    }
}