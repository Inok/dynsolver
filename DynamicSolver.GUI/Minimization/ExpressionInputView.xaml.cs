using System.Windows;
using System.Windows.Controls;
using DynamicSolver.ViewModel.Minimization;

namespace DynamicSolver.GUI.Minimization
{
    /// <summary>
    /// Interaction logic for ExpressionInputView.xaml
    /// </summary>
    public partial class ExpressionInputView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(ExpressionInputViewModel), typeof(ExpressionInputView), new PropertyMetadata(default(ExpressionInputViewModel)));

        public ExpressionInputViewModel ViewModel
        {
            get { return (ExpressionInputViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public ExpressionInputView()
        {
            InitializeComponent();
        }
    }
}