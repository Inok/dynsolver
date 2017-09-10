using System.Windows;
using System.Windows.Controls;
using DynamicSolver.App.ViewModel.DynamicSystem;
using ReactiveUI;

namespace DynamicSolver.App.DynamicSystem
{
    public partial class BatchModellingSettingsView : UserControl, IViewFor<BatchModellingSettingsViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(BatchModellingSettingsViewModel), typeof(BatchModellingSettingsView), new PropertyMetadata(default(BatchModellingSettingsViewModel)));

        public BatchModellingSettingsViewModel ViewModel
        {
            get => (BatchModellingSettingsViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        
        public BatchModellingSettingsView()
        {
            InitializeComponent();
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (BatchModellingSettingsViewModel) value;
        }
    }
}
