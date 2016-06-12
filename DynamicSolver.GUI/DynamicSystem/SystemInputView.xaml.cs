using System.Windows;
using System.Windows.Controls;
using DynamicSolver.ViewModel.DynamicSystem;

namespace DynamicSolver.GUI.DynamicSystem
{
    public partial class SystemInputView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(DynamicSystemInputViewModel), typeof(SystemInputView), new PropertyMetadata(default(DynamicSystemInputViewModel)));

        public DynamicSystemInputViewModel ViewModel
        {
            get { return (DynamicSystemInputViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public SystemInputView()
        {
            InitializeComponent();
        }
    }
}