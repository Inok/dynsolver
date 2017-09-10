using System.Windows;
using System.Windows.Controls;
using DynamicSolver.App.ViewModel.DynamicSystem;
using ReactiveUI;

namespace DynamicSolver.GUI.DynamicSystem
{
    public partial class DynamicSystemTaskView : UserControl, IViewFor<DynamicSystemTaskViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(DynamicSystemTaskViewModel), typeof(DynamicSystemTaskView), new PropertyMetadata(default(DynamicSystemTaskViewModel)));

        public DynamicSystemTaskViewModel ViewModel
        {
            get => (DynamicSystemTaskViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public DynamicSystemTaskView()
        {
            InitializeComponent();
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (DynamicSystemTaskViewModel) value;
        }
    }
}