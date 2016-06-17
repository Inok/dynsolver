using System;
using System.Reactive.Disposables;
using ReactiveUI;

namespace DynamicSolver.ViewModel.Common.Busy
{
    public class BusyIndicatorViewModel : ReactiveObject
    {
        private bool _isBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            set { this.RaiseAndSetIfChanged(ref _isBusy, value); }
        }

        public IDisposable CreateScope()
        {
            IsBusy = true;

            return Disposable.Create(() => IsBusy = false);
        }
    }
}