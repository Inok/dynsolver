using DynamicSolver.App.ViewModel;
using ReactiveUI;

namespace DynamicSolver.App
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
