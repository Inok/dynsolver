using System.Windows;
using System.Windows.Controls;
using DynamicSolver.App.ViewModel.DynamicSystem;
using ReactiveUI;

namespace DynamicSolver.GUI.DynamicSystem
{
    public partial class ModellingSettingsView : UserControl, IViewFor<ModellingSettingsViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(ModellingSettingsViewModel), typeof(ModellingSettingsView), new PropertyMetadata(default(ModellingSettingsViewModel)));

        public ModellingSettingsViewModel ViewModel
        {
            get => (ModellingSettingsViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public ModellingSettingsView()
        {
            InitializeComponent();
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ModellingSettingsViewModel) value;
        }
    }
}