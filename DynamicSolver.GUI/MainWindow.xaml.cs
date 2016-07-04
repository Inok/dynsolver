using DynamicSolver.ViewModel;
using ReactiveUI;

namespace DynamicSolver.GUI
{
    public partial class MainWindow
    {
        public IScreen ApplicationScreen { get; }

        public MainWindow()
        {
            ApplicationScreen = new ApplicationBootstraper(modules: new[] {new ViewsRegistrationModule()});

            InitializeComponent();
        }
    }
}
