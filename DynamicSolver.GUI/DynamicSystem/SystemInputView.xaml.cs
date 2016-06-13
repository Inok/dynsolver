using System.Windows;
using System.Windows.Controls;
using DynamicSolver.ViewModel.DynamicSystem;

namespace DynamicSolver.GUI.DynamicSystem
{
    public partial class SystemInputView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(DynamicSystemTaskViewModel), typeof(SystemInputView), new PropertyMetadata(default(DynamicSystemTaskViewModel)));

        public DynamicSystemTaskViewModel ViewModel
        {
            get { return (DynamicSystemTaskViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public SystemInputView()
        {
            InitializeComponent();
        }
    }
}