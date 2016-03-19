using System.Windows;
using System.Windows.Controls;
using DynamicSolver.ViewModel.DynamicSystem;

namespace DynamicSolver.GUI.DynamicSystem
{
    /// <summary>
    /// Interaction logic for ExpressionInputView.xaml
    /// </summary>
    public partial class SystemInputView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(SystemInputViewModel), typeof(SystemInputView), new PropertyMetadata(default(SystemInputViewModel)));

        public SystemInputViewModel ViewModel
        {
            get { return (SystemInputViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public SystemInputView()
        {
            ViewModel = new SystemInputViewModel(new ExpressionParser.Parser.ExpressionParser());

            InitializeComponent();
        }
    }
}