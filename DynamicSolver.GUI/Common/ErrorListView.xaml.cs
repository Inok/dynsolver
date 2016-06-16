using System.Windows;
using System.Windows.Controls;
using DynamicSolver.ViewModel.Common.ErrorList;
using ReactiveUI;

namespace DynamicSolver.GUI.Common
{
    public partial class ErrorListView : UserControl, IViewFor<ErrorListViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(ErrorListViewModel), typeof(ErrorListView), new PropertyMetadata(default(ErrorListViewModel)));

        public ErrorListViewModel ViewModel
        {
            get { return (ErrorListViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ErrorListViewModel) value; }
        }

        public ErrorListView()
        {
            InitializeComponent();
        }
    }
}
