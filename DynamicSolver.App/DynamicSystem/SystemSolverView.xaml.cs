using System.Windows;
using System.Windows.Controls;
using DynamicSolver.App.ViewModel.DynamicSystem;
using ReactiveUI;

namespace DynamicSolver.App.DynamicSystem
{
    public partial class SystemSolverView : UserControl, IViewFor<SystemSolverViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(SystemSolverViewModel), typeof(SystemSolverView), new PropertyMetadata(default(SystemSolverViewModel)));

        public SystemSolverViewModel ViewModel
        {
            get => (SystemSolverViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (SystemSolverViewModel) value;
        }

        public SystemSolverView()
        {
            InitializeComponent();            
        }
    }
}
