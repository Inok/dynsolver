using System.Windows;
using System.Windows.Controls;
using DynamicSolver.ViewModel.Common.Busy;
using ReactiveUI;

namespace DynamicSolver.GUI.Common
{
    public partial class BusyIndicatorView : UserControl, IViewFor<BusyIndicatorViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(BusyIndicatorViewModel), typeof(BusyIndicatorView), new PropertyMetadata(default(BusyIndicatorViewModel)));

        public BusyIndicatorViewModel ViewModel
        {
            get { return (BusyIndicatorViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (BusyIndicatorViewModel) value; }
        }

        public BusyIndicatorView()
        {
            InitializeComponent();
        }
    }
}
