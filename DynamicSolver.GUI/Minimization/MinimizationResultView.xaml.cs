using System.Windows;
using System.Windows.Controls;
using DynamicSolver.ViewModel.Minimization;

namespace DynamicSolver.GUI.Minimization
{
    public partial class MinimizationResultView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(MinimizationResultViewModel), typeof(MinimizationResultView), new PropertyMetadata(default(MinimizationResultViewModel)));

        public MinimizationResultViewModel ViewModel
        {
            get { return (MinimizationResultViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public MinimizationResultView()
        {
            InitializeComponent();
        }
    }
}
