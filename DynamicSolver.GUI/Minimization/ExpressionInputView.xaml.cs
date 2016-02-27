using System.Windows.Controls;
using DynamicSolver.ViewModel.Minimization;

namespace DynamicSolver.GUI.Minimization
{
    /// <summary>
    /// Interaction logic for ExpressionInputView.xaml
    /// </summary>
    public partial class ExpressionInputView : UserControl
    {
        public ExpressionInputViewModel ViewModel { get; }

        public ExpressionInputView()
        {
            ViewModel = new ExpressionInputViewModel(new ExpressionParser.Parser.ExpressionParser());
            InitializeComponent();
        }
    }
}