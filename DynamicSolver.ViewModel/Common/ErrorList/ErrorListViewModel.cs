using System.Reactive.Linq;
using JetBrains.Annotations;
using ReactiveUI;

namespace DynamicSolver.ViewModel.Common.ErrorList
{
    public class ErrorListViewModel : ReactiveObject
    {
        [NotNull, ItemNotNull]
        public IReactiveList<ErrorViewModel> Errors { get; }

        private readonly ObservableAsPropertyHelper<bool> _hasErrors;
        public bool HasErrors => _hasErrors.Value;


        public ErrorListViewModel()
        {
            Errors = new ReactiveList<ErrorViewModel>
            {
                ChangeTrackingEnabled = true
            };

            _hasErrors = Errors.IsEmptyChanged.Select(x => !x).ToProperty(this, x => x.HasErrors);
        }
    }
}