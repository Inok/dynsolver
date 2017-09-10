using System;
using JetBrains.Annotations;
using ReactiveUI;

namespace DynamicSolver.App.ViewModel.Common.Select
{
    public class SelectViewModel<T> : ReactiveObject
    {
        private string _name;
        private SelectItemViewModel<T> _selectedItem;

        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }

        public IReactiveList<SelectItemViewModel<T>> Items { get; }

        public SelectItemViewModel<T> SelectedItem
        {
            get { return _selectedItem; }
            set { this.RaiseAndSetIfChanged(ref _selectedItem, value); }
        }

        public SelectViewModel(bool enableItemsChangeTracking = true)
        {
            Items = new ReactiveList<SelectItemViewModel<T>>
            {
                ChangeTrackingEnabled = enableItemsChangeTracking
            };
        }

        public void AddItem([NotNull] string name, T value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            Items.Add(new SelectItemViewModel<T>(name, value));
        }
    }
}