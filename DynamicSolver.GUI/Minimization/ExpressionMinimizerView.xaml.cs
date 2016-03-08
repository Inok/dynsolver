using System.Windows.Controls;
using DynamicSolver.ViewModel.Minimization;

namespace DynamicSolver.GUI.Minimization
{
    /// <summary>
    /// Interaction logic for ExpressionMinimizerView.xaml
    /// </summary>
    public partial class ExpressionMinimizerView : UserControl
    {
        public ExpressionMinimizerViewModel ViewModel { get; set; }

        public ExpressionMinimizerView()
        {
            ViewModel = new ExpressionMinimizerViewModel();

            InitializeComponent();
        }
    }
}
