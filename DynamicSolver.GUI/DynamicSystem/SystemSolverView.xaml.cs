using System.Windows.Controls;
using DynamicSolver.ViewModel.DynamicSystem;

namespace DynamicSolver.GUI.DynamicSystem
{
    public partial class SystemSolverView : UserControl
    {
        public SystemSolverViewModel ViewModel { get; set; }

        public SystemSolverView()
        {
            ViewModel = new SystemSolverViewModel();

            InitializeComponent();

            ViewModel.Plotter = this.Plotter;
        }
    }
}
